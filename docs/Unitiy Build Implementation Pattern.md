# Unity Addressables Build Pipeline Pattern

### **Table of Contents**

1. [Overview](#overview)
    - [Key Features](#key-features)
    - [Use Cases](#use-cases)
    - [Benefits](#benefits)
2. [Technology Stack](#technology-stack)
3. [Directory Structure](#directory-structure)
4. [Configuration](#configuration)
    - [addressables-config.yaml](#addressables-configyaml)
    - [build-profiles.json](#build-profilesjson)
5. [Code](#code)
    - [Editor/BuildScripts.cs](#editorbuildscriptscs)
    - [Editor/AddressablesValidator.cs](#editoraddressablesvalidatorcs)
    - [Editor/BuildManifestGenerator.cs](#editorbuildmanifestgeneratorcs)
6. [Tests](#tests)
    - [Tests/Editor/BuildScriptsTests.cs](#testseditorbuildsscriptstestscs)
    - [Tests/PlayMode/AddressablesLoadTests.cs](#testsplaymodeaddressablesloadtestscs)
7. [CI/CD Pipeline with GitHub Actions](#cicd-pipeline-with-github-actions)
8. [Infrastructure as Code (IaC)](#infrastructure-as-code-iac)
    - [Terraform](#terraform)
    - [AWS CDK](#aws-cdk)
9. [Next Steps](#next-steps)

---

### **Overview**

The **Unity Addressables Build Pipeline Pattern** provides a production-ready automation framework for building, validating, and deploying Unity Addressable assets across multiple platforms and environments. This pattern orchestrates headless Unity builds through CI/CD pipelines and delivers content to AWS cloud infrastructure for global distribution.

#### **Key Features**

- **Multi-Platform Builds**: Automated builds for iOS, Android, WebGL, and Windows with platform-specific asset optimization.
- **Environment Isolation**: Separate Development, Staging, and Production environments with independent catalogs and approval workflows.
- **Automated Quality Gates**: Pre-build validation, bundle size verification, dependency analysis, and integrity checks.
- **Cloud-Native Distribution**: Direct integration with AWS S3 and CloudFront for scalable content delivery.

#### **Use Cases**

- **Live Service Games**: Ship cosmetics, events, and content updates without client patches.
- **Mobile Game Optimization**: Reduce APK/IPA size by streaming assets on-demand.
- **Cross-Platform Content**: Manage platform-specific bundles with unified pipelines.
- **Feature Flag Systems**: Deploy experimental content to targeted player segments using catalog labels.

#### **Benefits**

- **Faster Iteration**: Content teams deploy new assets in hours instead of days.
- **Reduced Download Size**: Players only download changed bundles, not full builds.
- **Cost Efficiency**: CDN caching and compression minimize bandwidth costs.
- **Production Safety**: Staged rollouts with automated testing catch issues before reaching players.

---

### Technology Stack

- **Game Engine**:
    - **Unity 2022.3 LTS** with Addressables Package 1.21.0+

- **Programming Languages**:
    - **C#** for Unity Editor build scripts and automation
    - **Bash/Shell** for CI/CD orchestration scripts

- **Unity Packages**:
    - **com.unity.addressables**: Asset management and dynamic content loading
    - **com.unity.scriptablebuildpipeline**: Low-level build pipeline control
    - **com.unity.testframework**: Automated testing infrastructure

- **Cloud Services**:
    - **Amazon S3**: Origin storage for asset bundles and catalogs
    - **Amazon CloudFront**: Global CDN for low-latency content delivery
    - **AWS IAM**: Access control and security policies
    - **Amazon CloudWatch**: Monitoring and operational metrics

- **CI/CD Platform**:
    - **GitHub Actions**: Workflow automation and build orchestration
    - **GameCI Unity Builder**: Docker-based headless Unity build containers

- **Infrastructure as Code (IaC)**:
    - **Terraform**: Declarative infrastructure provisioning
    - **AWS CDK (Python)**: Programmatic infrastructure definitions

- **Testing**:
    - **Unity Test Framework**: PlayMode and EditMode test execution
    - **NUnit**: Unit test assertions and test organization

- **Utilities**:
    - **AWS CLI**: S3 upload and CloudFront invalidation
    - **jq**: JSON parsing for manifests and validation

---

### Directory Structure

```
/UnityProject
│
├── /Assets
│   ├── /Editor
│   │   ├── BuildScripts.cs
│   │   ├── AddressablesValidator.cs
│   │   └── BuildManifestGenerator.cs
│   │
│   ├── /AddressableAssetsData
│   │   ├── AddressableAssetSettings.asset
│   │   └── /AssetGroups
│   │
│   └── /AddressableAssets
│       ├── /Characters
│       ├── /Environments
│       └── /UI
│
├── /Tests
│   ├── /Editor
│   │   └── BuildScriptsTests.cs
│   └── /PlayMode
│       └── AddressablesLoadTests.cs
│
├── /ServerData
│   ├── /android
│   ├── /ios
│   ├── /webgl
│   └── /windows
│
├── /.github
│   └── /workflows
│       └── build-addressables.yml
│
├── /iac
│   ├── /terraform
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   └── outputs.tf
│   └── /cdk
│       ├── app.py
│       └── requirements.txt
│
├── /config
│   ├── addressables-config.yaml
│   └── build-profiles.json
│
└── .gitignore
```

---

### Configuration

#### addressables-config.yaml

```yaml
# Unity Addressables Build Configuration

project:
  name: "MyGame"
  unity_version: "2022.3.20f1"

platforms:
  android:
    enabled: true
    build_target: "Android"
    max_bundle_size_mb: 50
    
  ios:
    enabled: true
    build_target: "iOS"
    max_bundle_size_mb: 50
    
  webgl:
    enabled: true
    build_target: "WebGL"
    max_bundle_size_mb: 25
    
  windows:
    enabled: true
    build_target: "StandaloneWindows64"
    max_bundle_size_mb: 100

environments:
  development:
    s3_bucket: "mygame-addressables-dev"
    cloudfront_id: "E1DEVEXAMPLE"
    cdn_url: "https://dev-cdn.mygame.com"
    
  staging:
    s3_bucket: "mygame-addressables-stage"
    cloudfront_id: "E2STAGEEXAMPLE"
    cdn_url: "https://stage-cdn.mygame.com"
    
  production:
    s3_bucket: "mygame-addressables-prod"
    cloudfront_id: "E3PRODEXAMPLE"
    cdn_url: "https://cdn.mygame.com"

validation:
  max_bundle_size_mb: 100
  check_duplicates: true
  verify_dependencies: true
```

#### build-profiles.json

```json
{
  "profiles": [
    {
      "name": "Development",
      "id": "dev",
      "cdn_url": "https://dev-cdn.mygame.com"
    },
    {
      "name": "Staging",
      "id": "staging",
      "cdn_url": "https://stage-cdn.mygame.com"
    },
    {
      "name": "Production",
      "id": "prod",
      "cdn_url": "https://cdn.mygame.com"
    }
  ]
}
```

---

### Code

#### Editor/BuildScripts.cs

```csharp
// Assets/Editor/BuildScripts.cs

using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace BuildPipeline
{
    public static class BuildScripts
    {
        public static void BuildAddressablesForPlatform()
        {
            string[] args = Environment.GetCommandLineArgs();
            string buildTarget = GetArg(args, "-buildTarget", "StandaloneWindows64");
            string profile = GetArg(args, "-addressablesProfile", "Default");
            
            Debug.Log($"Building Addressables: {buildTarget} | {profile}");
            
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            settings.activeProfileId = settings.profileSettings.GetProfileId(profile);
            
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            
            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.LogError($"Build failed: {result.Error}");
                EditorApplication.Exit(1);
            }
            
            Debug.Log($"Build completed in {result.Duration}s");
            EditorApplication.Exit(0);
        }
        
        private static string GetArg(string[] args, string name, string defaultValue)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name) return args[i + 1];
            }
            return defaultValue;
        }
    }
}
```

#### Editor/AddressablesValidator.cs

```csharp
// Assets/Editor/AddressablesValidator.cs

using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace BuildPipeline
{
    public class AddressablesValidator
    {
        private List<string> errors = new List<string>();
        
        public bool ValidateSettings()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found");
                return false;
            }
            
            ValidateGroups(settings);
            ValidateProfiles(settings);
            
            if (errors.Count > 0)
            {
                foreach (string error in errors)
                {
                    Debug.LogError($"Validation: {error}");
                }
                return false;
            }
            
            Debug.Log("Validation passed");
            return true;
        }
        
        private void ValidateGroups(AddressableAssetSettings settings)
        {
            foreach (var group in settings.groups)
            {
                if (group == null) continue;
                if (group.entries.Count == 0)
                {
                    errors.Add($"Group '{group.Name}' is empty");
                }
            }
        }
        
        private void ValidateProfiles(AddressableAssetSettings settings)
        {
            if (settings.profileSettings.profiles.Count == 0)
            {
                errors.Add("No profiles configured");
            }
        }
    }
}
```

#### Editor/BuildManifestGenerator.cs

```csharp
// Assets/Editor/BuildManifestGenerator.cs

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BuildPipeline
{
    [Serializable]
    public class BuildManifest
    {
        public string version;
        public string platform;
        public string timestamp;
        public string environment;
    }
    
    public class BuildManifestGenerator
    {
        public void GenerateManifest(BuildTarget target, string profile, string outputPath)
        {
            BuildManifest manifest = new BuildManifest
            {
                version = Application.version,
                platform = target.ToString(),
                timestamp = DateTime.UtcNow.ToString("o"),
                environment = profile
            };
            
            string json = JsonUtility.ToJson(manifest, true);
            string manifestPath = Path.Combine(outputPath, "manifest.json");
            File.WriteAllText(manifestPath, json);
            
            Debug.Log($"Manifest generated: {manifestPath}");
        }
    }
}
```

---

### Tests

#### Tests/Editor/BuildScriptsTests.cs

```csharp
// Tests/Editor/BuildScriptsTests.cs

using NUnit.Framework;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace BuildPipeline.Tests
{
    public class BuildScriptsTests
    {
        [Test]
        public void AddressablesSettings_ShouldExist()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            Assert.IsNotNull(settings, "AddressableAssetSettings should be configured");
        }
        
        [Test]
        public void AddressablesSettings_ShouldHaveProfiles()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            Assert.Greater(settings.profileSettings.profiles.Count, 0, 
                "At least one profile should be configured");
        }
        
        [Test]
        public void Validator_ShouldPass_WithValidSettings()
        {
            var validator = new AddressablesValidator();
            bool result = validator.ValidateSettings();
            Assert.IsTrue(result, "Validation should pass with valid settings");
        }
    }
}
```

#### Tests/PlayMode/AddressablesLoadTests.cs

```csharp
// Tests/PlayMode/AddressablesLoadTests.cs

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TestTools;

namespace BuildPipeline.Tests
{
    public class AddressablesLoadTests
    {
        [UnityTest]
        public IEnumerator LoadAsset_ShouldSucceed_WhenAssetExists()
        {
            var handle = Addressables.LoadAssetAsync<GameObject>("TestAsset");
            yield return handle;
            
            Assert.AreEqual(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded, 
                handle.Status, "Asset should load successfully");
            
            Addressables.Release(handle);
        }
        
        [UnityTest]
        public IEnumerator InitializeAddressables_ShouldComplete()
        {
            var initHandle = Addressables.InitializeAsync();
            yield return initHandle;
            
            Assert.IsTrue(initHandle.IsDone, "Addressables should initialize");
        }
    }
}
```

---

### **CI/CD Pipeline with GitHub Actions**

#### GitHub Actions Workflow

```yaml
# .github/workflows/build-addressables.yml

name: Build and Deploy Addressables

on:
  push:
    branches: [main, develop]
  workflow_dispatch:
    inputs:
      environment:
        description: 'Environment'
        required: true
        type: choice
        options: [development, staging, production]
      platforms:
        description: 'Platforms (comma-separated)'
        required: true
        default: 'android,ios,webgl,windows'

env:
  UNITY_VERSION: 2022.3.20f1

jobs:
  build:
    name: Build ${{ matrix.platform }}
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        include:
          - platform: Android
            os: ubuntu-latest
            s3-prefix: android
          - platform: iOS
            os: macos-latest
            s3-prefix: ios
          - platform: WebGL
            os: ubuntu-latest
            s3-prefix: webgl
          - platform: Windows
            os: ubuntu-latest
            s3-prefix: windows
    
    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          lfs: true
          
      - name: Cache Library
        uses: actions/cache@v3
        with:
          path: Library
          key: Library-${{ matrix.platform }}-${{ hashFiles('Packages/manifest.json') }}
          
      - name: Build Addressables
        uses: game-ci/unity-builder@v4
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
          UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
          UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}
        with:
          projectPath: .
          targetPlatform: ${{ matrix.platform }}
          buildMethod: BuildPipeline.BuildScripts.BuildAddressablesForPlatform
          customParameters: -addressablesProfile ${{ github.event.inputs.environment || 'development' }}
          
      - name: Validate Build
        run: |
          if [ ! -d "ServerData/${{ matrix.s3-prefix }}" ]; then
            echo "Build output not found"
            exit 1
          fi
          
          find ServerData/${{ matrix.s3-prefix }} -name "*.bundle" -size +100M -exec ls -lh {} \;
          if find ServerData/${{ matrix.s3-prefix }} -name "*.bundle" -size +100M | grep -q .; then
            echo "Bundle exceeds 100MB"
            exit 1
          fi
          
      - name: Upload Artifacts
        uses: actions/upload-artifact@v4
        with:
          name: addressables-${{ matrix.platform }}-${{ github.run_number }}
          path: ServerData/${{ matrix.s3-prefix }}/
          retention-days: 30

  test:
    name: Test ${{ matrix.platform }}
    needs: build
    runs-on: ubuntu-latest
    strategy:
      matrix:
        platform: [Android, iOS, WebGL, Windows]
    
    steps:
      - name: Checkout
        uses: actions/checkout@v4
        
      - name: Download Artifacts
        uses: actions/download-artifact@v4
        with:
          name: addressables-${{ matrix.platform }}-${{ github.run_number }}
          path: ServerData/
          
      - name: Run Tests
        uses: game-ci/unity-test-runner@v4
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
        with:
          projectPath: .
          testMode: PlayMode

  deploy:
    name: Deploy to AWS
    needs: test
    runs-on: ubuntu-latest
    if: github.event_name != 'pull_request'
    environment: ${{ github.event.inputs.environment || 'development' }}
    
    steps:
      - name: Download All Artifacts
        uses: actions/download-artifact@v4
        with:
          path: artifacts/
          
      - name: Configure AWS
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1
          
      - name: Set Environment
        run: |
          ENV="${{ github.event.inputs.environment || 'development' }}"
          case $ENV in
            production)
              echo "S3_BUCKET=mygame-addressables-prod" >> $GITHUB_ENV
              echo "CLOUDFRONT_ID=${{ secrets.PROD_CLOUDFRONT_ID }}" >> $GITHUB_ENV
              ;;
            staging)
              echo "S3_BUCKET=mygame-addressables-stage" >> $GITHUB_ENV
              echo "CLOUDFRONT_ID=${{ secrets.STAGE_CLOUDFRONT_ID }}" >> $GITHUB_ENV
              ;;
            *)
              echo "S3_BUCKET=mygame-addressables-dev" >> $GITHUB_ENV
              echo "CLOUDFRONT_ID=${{ secrets.DEV_CLOUDFRONT_ID }}" >> $GITHUB_ENV
              ;;
          esac
          echo "VERSION=${{ github.run_number }}" >> $GITHUB_ENV
          
      - name: Upload to S3
        run: |
          for platform in android ios webgl windows; do
            ARTIFACT_DIR="artifacts/addressables-${platform^}-${{ github.run_number }}"
            
            if [ -d "$ARTIFACT_DIR" ]; then
              echo "Uploading ${platform}..."
              
              aws s3 sync "$ARTIFACT_DIR" \
                "s3://${S3_BUCKET}/${platform}/v${VERSION}/" \
                --metadata "version=${VERSION},commit=${{ github.sha }}" \
                --cache-control "public, max-age=31536000" \
                --exclude "*.json" --exclude "*.hash"
              
              aws s3 sync "$ARTIFACT_DIR" \
                "s3://${S3_BUCKET}/${platform}/v${VERSION}/" \
                --cache-control "public, max-age=600" \
                --exclude "*" --include "*.json" --include "*.hash"
              
              aws s3 sync "s3://${S3_BUCKET}/${platform}/v${VERSION}/" \
                "s3://${S3_BUCKET}/${platform}/latest/"
            fi
          done
          
      - name: Invalidate CloudFront
        run: |
          aws cloudfront create-invalidation \
            --distribution-id "${CLOUDFRONT_ID}" \
            --paths "/*/latest/*"
```

---

### Infrastructure as Code (IaC)

#### **Terraform**

##### iac/terraform/variables.tf

```hcl
# iac/terraform/variables.tf

variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Project name"
  type        = string
  default     = "mygame-addressables"
}

variable "environments" {
  description = "List of environments"
  type        = list(string)
  default     = ["dev", "stage", "prod"]
}
```

##### iac/terraform/outputs.tf

```hcl
# iac/terraform/outputs.tf

output "s3_buckets" {
  value = {
    for env in var.environments :
    env => aws_s3_bucket.addressables[env].id
  }
  description = "S3 bucket names for each environment"
}

output "cloudfront_distributions" {
  value = {
    for env in var.environments :
    env => aws_cloudfront_distribution.cdn[env].id
  }
  description = "CloudFront distribution IDs"
}

output "cdn_urls" {
  value = {
    for env in var.environments :
    env => "https://${aws_cloudfront_distribution.cdn[env].domain_name}"
  }
  description = "CDN URLs for each environment"
}
```

##### iac/terraform/main.tf

```hcl
# iac/terraform/main.tf

provider "aws" {
  region = var.aws_region
}

# S3 Buckets for each environment
resource "aws_s3_bucket" "addressables" {
  for_each = toset(var.environments)
  
  bucket = "${var.project_name}-${each.value}"
  
  tags = {
    Environment = each.value
    Project     = var.project_name
  }
}

resource "aws_s3_bucket_versioning" "addressables" {
  for_each = toset(var.environments)
  
  bucket = aws_s3_bucket.addressables[each.value].id
  
  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_public_access_block" "addressables" {
  for_each = toset(var.environments)
  
  bucket = aws_s3_bucket.addressables[each.value].id
  
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# CloudFront Origin Access Control
resource "aws_cloudfront_origin_access_control" "addressables" {
  for_each = toset(var.environments)
  
  name                              = "${var.project_name}-${each.value}-oac"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

# CloudFront Distribution
resource "aws_cloudfront_distribution" "cdn" {
  for_each = toset(var.environments)
  
  enabled             = true
  is_ipv6_enabled     = true
  comment             = "Addressables CDN for ${each.value}"
  default_root_object = ""
  
  origin {
    domain_name              = aws_s3_bucket.addressables[each.value].bucket_regional_domain_name
    origin_id                = "S3-${each.value}"
    origin_access_control_id = aws_cloudfront_origin_access_control.addressables[each.value].id
  }
  
  default_cache_behavior {
    allowed_methods        = ["GET", "HEAD", "OPTIONS"]
    cached_methods         = ["GET", "HEAD"]
    target_origin_id       = "S3-${each.value}"
    viewer_protocol_policy = "redirect-to-https"
    compress               = true
    
    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
    }
    
    min_ttl     = 0
    default_ttl = each.value == "prod" ? 86400 : 3600
    max_ttl     = 31536000
  }
  
  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }
  
  viewer_certificate {
    cloudfront_default_certificate = true
  }
  
  tags = {
    Environment = each.value
    Project     = var.project_name
  }
}

# S3 Bucket Policy for CloudFront
resource "aws_s3_bucket_policy" "addressables" {
  for_each = toset(var.environments)
  
  bucket = aws_s3_bucket.addressables[each.value].id
  
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "AllowCloudFrontAccess"
        Effect = "Allow"
        Principal = {
          Service = "cloudfront.amazonaws.com"
        }
        Action   = "s3:GetObject"
        Resource = "${aws_s3_bucket.addressables[each.value].arn}/*"
        Condition = {
          StringEquals = {
            "AWS:SourceArn" = aws_cloudfront_distribution.cdn[each.value].arn
          }
        }
      }
    ]
  })
}
```

---

#### **AWS CDK**

##### iac/cdk/app.py

```python
# iac/cdk/app.py

from aws_cdk import App
from s3_stack import S3Stack
from cloudfront_stack import CloudFrontStack

app = App()

environments = ["dev", "stage", "prod"]

for env in environments:
    s3_stack = S3Stack(app, f"AddressablesS3Stack-{env}", env=env)
    cloudfront_stack = CloudFrontStack(
        app, 
        f"AddressablesCDNStack-{env}",
        bucket=s3_stack.bucket,
        env_name=env
    )

app.synth()
```

##### iac/cdk/s3_stack.py

```python
# iac/cdk/s3_stack.py

from aws_cdk import (
    Stack,
    RemovalPolicy,
    aws_s3 as s3,
)
from constructs import Construct

class S3Stack(Stack):
    def __init__(self, scope: Construct, construct_id: str, env: str, **kwargs):
        super().__init__(scope, construct_id, **kwargs)
        
        self.bucket = s3.Bucket(
            self, 
            f"AddressablesBucket-{env}",
            bucket_name=f"mygame-addressables-{env}",
            versioned=True,
            block_public_access=s3.BlockPublicAccess.BLOCK_ALL,
            removal_policy=RemovalPolicy.RETAIN,
            lifecycle_rules=[
                s3.LifecycleRule(
                    enabled=True,
                    noncurrent_version_expiration=Duration.days(90)
                )
            ]
        )
```

##### iac/cdk/cloudfront_stack.py

```python
# iac/cdk/cloudfront_stack.py

from aws_cdk import (
    Stack,
    Duration,
    aws_cloudfront as cloudfront,
    aws_cloudfront_origins as origins,
    aws_s3 as s3,
)
from constructs import Construct

class CloudFrontStack(Stack):
    def __init__(
        self, 
        scope: Construct, 
        construct_id: str, 
        bucket: s3.IBucket,
        env_name: str,
        **kwargs
    ):
        super().__init__(scope, construct_id, **kwargs)
        
        cache_ttl = Duration.days(1) if env_name == "prod" else Duration.hours(1)
        
        self.distribution = cloudfront.Distribution(
            self,
            f"AddressablesCDN-{env_name}",
            default_behavior=cloudfront.BehaviorOptions(
                origin=origins.S3Origin(bucket),
                viewer_protocol_policy=cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                cache_policy=cloudfront.CachePolicy(
                    self,
                    f"CachePolicy-{env_name}",
                    default_ttl=cache_ttl,
                    max_ttl=Duration.days(365),
                    min_ttl=Duration.seconds(0),
                    enable_accept_encoding_gzip=True,
                    enable_accept_encoding_brotli=True
                ),
                compress=True
            ),
            price_class=cloudfront.PriceClass.PRICE_CLASS_100
        )
```

##### iac/cdk/requirements.txt

```plaintext
aws-cdk-lib==2.100.0
constructs>=10.0.0,<11.0.0
```

---

### **Next Steps**

1. **Configure GitHub Secrets**:
    - Add `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY`
    - Add `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`
    - Add environment-specific `CLOUDFRONT_ID` values

2. **Deploy Infrastructure**:
    ```bash
    # Using Terraform
    cd iac/terraform
    terraform init
    terraform apply
    
    # Using AWS CDK
    cd iac/cdk
    pip install -r requirements.txt
    cdk bootstrap
    cdk deploy --all
    ```

3. **Configure Unity Profiles**:
    - Open Unity Editor
    - Navigate to Window → Asset Management → Addressables → Profiles
    - Create profiles for Development, Staging, Production
    - Set Remote Load Paths to match CDN URLs

4. **Test Local Build**:
    ```bash
    /Applications/Unity/Hub/Editor/2022.3.20f1/Unity.app/Contents/MacOS/Unity \
      -quit -batchmode -projectPath . \
      -executeMethod BuildPipeline.BuildScripts.BuildAddressablesForPlatform \
      -buildTarget Android \
      -addressablesProfile Development
    ```

5. **Trigger Pipeline**:
    - Push to `main` branch or use workflow_dispatch
    - Monitor GitHub Actions for build progress
    - Verify S3 uploads and CloudFront distribution

6. **Monitor and Optimize**:
    - Set up CloudWatch dashboards for CDN metrics
    - Monitor cache hit ratios and bandwidth usage
    - Review bundle sizes and optimize large assets
    - Configure alerts for failed builds or deployments

7. **Implement Rollback**:
    - Store version history in S3 metadata
    - Create rollback workflow to copy previous version to `/latest/`
    - Test rollback procedure