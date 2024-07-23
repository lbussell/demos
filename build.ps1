
Write-Host
Write-Host

# Get a list of all Dockerfile names
$dockerfiles = Get-ChildItem -Path "." -Filter "*.Dockerfile"

foreach ($file in $dockerfiles) {
    Write-Host "Found Dockerfile to build: ${file}"
}

# Initialize an array to hold image details
$imageDetails = @()

foreach ($file in $dockerfiles) {
    # Extract base name for tag
    $tagName = $file.BaseName -replace '\.Dockerfile$'

    # Capture start time
    $startTime = Get-Date

    # Build the Docker image
    docker build --pull --no-cache -t $tagName -f $file.FullName .

    # Capture end time and calculate duration
    $endTime = Get-Date
    $duration = $endTime - $startTime
    $durationSeconds = [math]::Round($duration.TotalSeconds, 2)

    # Get the size of the built image
    $imageInfo = docker image inspect $tagName --format='{{.Size}}'
    $imageSizeMB = [math]::Round($imageInfo / 1MB, 2)

    # Get the size of each layer
    $layerInfos = docker history $tagName --format "{{.ID}}: {{.Size}}"

    # Create a custom object with image details
    $imageDetail = [PSCustomObject]@{
        TagName = $tagName
        BuildTimeSeconds = $durationSeconds
        ImageSizeMB = $imageSizeMB
        Layers = $layerInfos -join "`n"
    }

    # Add the details to the array
    $imageDetails += $imageDetail
}

# Print the details for each image
foreach ($detail in $imageDetails) {
    Write-Host "Image: $($detail.TagName)"
    Write-Host "Build Time: $($detail.BuildTimeSeconds) seconds"
    Write-Host "Image Size: $($detail.ImageSizeMB) MB"
    # Write-Host "Layers:"
    # Write-Host $($detail.Layers)
    Write-Host # Empty line for better readability
}

Write-Host
Write-Host
