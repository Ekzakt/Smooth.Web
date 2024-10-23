``` yaml 
name: Build and deploy ASP.Net Core app to Azure Web App - app-smooth-web-stage

on:
  push:
    branches:
      - staging
  workflow_dispatch:

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout repository (without submodules)
        uses: actions/checkout@v2
        with:
          submodules: false  # We'll manually handle submodule placement later

      - name: Clone Smooth.Shared submodule in the correct location
        run: |
          git clone https://github.com/Ekzakt/Smooth.Shared ../Smooth.Shared

      - name: Set up .NET Core
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '8.x'
          include-prerelease: true

      - name: Print working directory
        run: pwd

      - name: List files in workspace
        run: ls -R ${{ github.workspace }}

      # Restore dependencies, including for submodules
      - name: Restore dependencies
        working-directory: ${{ github.workspace }}/src/Smooth.Web
        run: dotnet restore

      # Build the Smooth.Web project
      - name: Build with dotnet
        working-directory: ${{ github.workspace }}/src/Smooth.Web
        run: dotnet build --configuration Release

      # Publish the Smooth.Web project
      - name: Publish with dotnet
        working-directory: ${{ github.workspace }}/src/Smooth.Web
        run: dotnet publish -c Release -o ${{ github.workspace }}/publish_output

      - name: Upload artifact for deployment job
        uses: actions/upload-artifact@v3
        with:
          name: .net-app
          path: ${{ github.workspace }}/publish_output

  deploy:
    runs-on: ubuntu-latest
    needs: build
    environment:
      name: 'Production'
      url: ${{ steps.deploy-to-webapp.outputs.webapp-url }}

    steps:
      - name: Download artifact from build job
        uses: actions/download-artifact@v3
        with:
          name: .net-app

      - name: Deploy to Azure Web App
        id: deploy-to-webapp
        uses: azure/webapps-deploy@v2
        with:
          app-name: 'app-smooth-web-stage'
          slot-name: 'Production'
          publish-profile: ${{ secrets.AZUREAPPSERVICE_PUBLISHPROFILE_B4965181552C4F59BCE5376DAEF51BC0 }}
          package: .
```

# Explanation of the Workflow File

## Context and Folder Structure
The workflow is designed to build and deploy the `Smooth.Web` project from your GitHub repository to an Azure Web App named `app-smooth-web-stage`. You have two main projects involved:
- **Smooth.Web**: The main application that is being built and deployed.
- **Smooth.Shared**: A shared submodule project that is referenced by `Smooth.Web`.

### Physical Folder Structure (On Local Machine)
- `Smooth.Sensation/`
  - `Smooth.Web/`: The folder containing the main web application.
  - `Smooth.Shared/`: A separate project referenced by `Smooth.Web`.

### GitHub Repository Structure
- The repository `Smooth.Web` includes the `Smooth.Web` project.
- The repository also uses `Smooth.Shared` as a Git submodule.

## Detailed Steps

### Build Job
1. **Checkout Repository (Without Submodules)**:
   - This step checks out the `Smooth.Web` repository, but not the submodules (`submodules: false`). The submodule (`Smooth.Shared`) will be handled manually in the next step.

2. **Clone Smooth.Shared Submodule in the Correct Location**:
   - Manually clones the `Smooth.Shared` submodule to the correct location (`../Smooth.Shared`). This step ensures that the folder structure matches what the solution file (`.sln`) expects.

3. **Set Up .NET Core**:
   - Sets up the .NET SDK version 8.x, including pre-release versions if necessary. This is required to build and publish the project.

4. **Print Working Directory** and **List Files in Workspace**:
   - These steps are for debugging purposes. They print the current working directory and list all files in the workspace to ensure everything is in the right place.

5. **Restore Dependencies**:
   - Restores the NuGet packages for the `Smooth.Web` project. The `working-directory` is set to `src/Smooth.Web` to ensure dependencies for `Smooth.Shared` are also restored, as it is referenced by `Smooth.Web`.

6. **Build the Smooth.Web Project**:
   - Builds the `Smooth.Web` project in Release mode. This compiles the code and prepares it for deployment.

7. **Publish the Smooth.Web Project**:
   - Publishes the `Smooth.Web` project to a specific folder (`publish_output`). This prepares the files needed for deployment to Azure.

8. **Upload Artifact for Deployment Job**:
   - Uploads the published files as an artifact named `.net-app`, which will be used in the subsequent deployment job.

### Deploy Job
1. **Download Artifact from Build Job**:
   - Downloads the `.net-app` artifact from the build job, which contains the published output of the `Smooth.Web` project.

2. **Deploy to Azure Web App**:
   - Deploys the downloaded artifact to the Azure Web App (`app-smooth-web-stage`). The deployment uses a publish profile (`AZUREAPPSERVICE_PUBLISHPROFILE_B4965181552C4F59BCE5376DAEF51BC0`), which needs to be set up as a GitHub secret for authentication.

## Important Points
- **Manual Submodule Handling**: Since GitHub Actions initially did not find the submodule correctly, we manually cloned the `Smooth.Shared` repository. This ensures that it is placed where the solution file expects it (`../Smooth.Shared` relative to `Smooth.Web`).
- **Publish Profile Secret**: The secret (`AZUREAPPSERVICE_PUBLISHPROFILE_B4965181552C4F59BCE5376DAEF51BC0`) must be configured in the GitHub repository. It contains credentials to deploy the application to Azure.
- **Relative Paths**: The structure of your repository and the manual handling of submodules ensures the build and restore commands can resolve paths like `..\Smooth.Shared`, which is essential for Visual Studio's project references to work during CI/CD.

This workflow allows you to successfully build and deploy your application while handling the complexities of submodules and ensuring compatibility between the GitHub Actions environment and your local Visual Studio setup.
