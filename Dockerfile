FROM azurelinuxpreview.azurecr.io/public/azurelinux/base/core:3.0

RUN tdnf install -y ca-certificates-base
