FROM scratch AS marinara
ADD https://github.com/microsoft/marinara.git /

FROM azurelinuxpreview.azurecr.io/public/azurelinux/base/core:3.0 AS builder

# Install Marinara (using --link makes it independent from dependencies below)
COPY --link --from=marinara scripts/ /usr/sbin

# Install Marinara depedencies
# See https://github.com/microsoft/marinara/blob/main/dockerfile-marinara
RUN tdnf install -y \
        shadow-utils \
        python3 \
    && tdnf clean all \
    && chmod +x /usr/sbin/*

RUN marinaracreate.py \
    --image-type "minimal-nonroot" \
    --azure-linux-version "3.0" \
    --location "/staging" \
    --add-packages "prebuilt-ca-certificates glibc libgcc libstdc++ openssl-libs zlib" \
    --packages-to-holdback "" \
    --user "app" \
    --user-uid "1654" \
    --user-gid "1654"

# .NET runtime-deps image
FROM scratch

ENV \
    # UID of the non-root user 'app'
    APP_UID=1654 \
    # Configure web servers to bind to port 8080 when present
    ASPNETCORE_HTTP_PORTS=8080 \
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true \
    # Set the invariant mode since ICU package isn't included (see https://github.com/dotnet/announcements/issues/20)
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

COPY --link --from=builder /staging/ /

# Workaround for https://github.com/moby/moby/issues/38710
COPY --link --from=builder --chown=1654:1654 /staging/home/ /home/

USER $APP_UID
