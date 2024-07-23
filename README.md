# Marinara Image Size Investigation

Size experiments for https://github.com/dotnet/dotnet-docker/issues/4771.

All images have the same set of dependencies installed, including `zlib`.
Uncompressed image sizes do not use `containerd` image store.

## Results

| Image Name | Compare to | Size (Uncompressed) | Change (Uncompressed) | Size (Compressed) | Diff (Compressed) | Build Time |
|---|---|---|---|---|---|---|
| cbl-mariner2.0-distroless         |                           | 22.69 MB | Baseline               | 10.34 MB  | Baseline              | 52.21s |
| azurelinux3.0-distroless          | cbl-mariner2.0-distroless | 25.46 MB | +2.77 MB / +12.2%      | 11.15 MB  | +0.81 MB / +7.83%     | 13.5s |
| azurelinux3.0-marinara            | azurelinux3.0-distroless  | 23.12 MB | -2.34 MB / -9.19%      | 9.69 MB   | **-1.46 MB / -13.1%** | 14s |
| "                                 | cbl-mariner2.0-distroless | "        | +0.43 MB / +1.90%      | "         | **-0.65 MB / -6.29%** | " |
| azurelinux3.0-marinara-python     | azurelinux3.0-marinara    | 23.12 MB | +0 MB / +0%            | 9.69 MB   | +0 MB / +0%           | 17.17s |

### Notes:
- Important results are bolded.
- `azurelinux3.0-marinara` and `marinara-python` are identical, as expected. There's no need to look further into that. Using Marinara directly instead of relying on the published Marinara image only adds time to the build process.
- `cbl-mariner2.0-distroless` and `azurelinux3.0-distroless` are 2.77 MB different. This change comes primarily from an increase in size of the following files:
    - `libcrypto.so.1.1.1k` (3.6 MB) -> `libcrypto.so.3.3.0` (5.3 MB)
    - `libc.so.6` (2.1 MB) -> `libc.so.6` (2.4 MB)
    -  You can see the diff in more detail [here](1.png).

### Where does the size improvement for Marinara come from?

`azurelinux3.0-distroless` and `azurelinux3.0-marinara` should be identical in output and (ideally) size.
`azurelinux3.0-distroless` is based on a base image instead of `scratch`, however, so we run the risk of overlapping files if we've changed anything compared to that base image.

List of packages installed by default on `azurelinuxpreview.azurecr.io/public/azurelinux/base/core:3.0` (at the time of this commit)

```
azurelinux-release           3.0-15.azl3   rpm
distroless-packages-minimal  3.0-5.azl3    rpm
filesystem                   1.1-21.azl3   rpm
prebuilt-ca-certificates     3.0.0-6.azl3  rpm
tzdata                       2024a-1.azl3  rpm
```

When we build `azurelinux3.0-distroless`, we get the following packages in the final image:

```
azurelinux-release           3.0-15.azl3           rpm
distroless-packages-minimal  3.0-5.azl3            rpm
filesystem                   1.1-21.azl3           rpm
glibc                        2.38-6.azl3           rpm
libgcc                       13.2.0-7.azl3         rpm
libstdc++                    13.2.0-7.azl3         rpm
openssl-libs                 3.3.0-1.azl3          rpm
prebuilt-ca-certificates     2415459:3.0.0-6.azl3  rpm
tzdata                       2024a-1.azl3          rpm
zlib                         1.3.1-1.azl3          rpm
```

Most of the size difference seems to come from the fact that ca-certs get copied over/updated in the final layer of `azurelinux3.0-distroless`.
The version reported by Syft here is different: `3.0.0-6.azl3` vs. `2415459:3.0.0-6.azl3`.
It's not clear to me if that version is any different or if the timestamp is significant, or if the contents of the package are substantially different.
The [package spec](https://github.com/microsoft/azurelinux/blob/3.0/SPECS/prebuilt-ca-certificates/prebuilt-ca-certificates.spec) hasn't changed, but the epoch (prefix) has, and I can't find where the epoch `2415459` may be coming from.
If the contents of the package were the same then we would have a negligible diff here.
You can see the diff of this more clearly in [this image](2.png).

## Conclusion

We could get the same benefits as using Marinara by simply squashing our final layer of the Azure Linux distroless image.
In fact, I did this with `azurelinux3.0-distroless-squashed` and got an image size of 23.12 MB uncompressed / 9.69 MB compressed.
That's identical to the result with Marinara.
In my opinion this splits up the decision into two separate factors:

### Using Marinara

- Pros: Simplicity of Dockerfile definition
- Cons: Lack of direct support

### Squashing final image layer

- Pros: Dockerfile will always have perfect size efficiency
- Cons: Lose base image layer sharing with other images based on the Azure Linux distroless base image, longer build time
