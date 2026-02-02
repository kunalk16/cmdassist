# Copyright (c) 2026 Kunal Karmakar
# Licensed under the MIT License. See LICENSE file in the project root for full license information.

#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Example environment setup script for CmdAssist

.DESCRIPTION
    This script demonstrates how to set up environment variables for different AI providers.
    Copy this file to setup-env.local.ps1 and customize with your actual API keys.

.NOTES
    This file is for demonstration purposes only. Never commit actual API keys to version control!
#>

Write-Host "🔧 Setting up CmdAssist Environment Variables" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠️  WARNING: This is an example file. Copy to 'setup-env.local.ps1' and add your real API keys." -ForegroundColor Yellow
Write-Host ""

# OpenAI Configuration
Write-Host "🤖 OpenAI Configuration:" -ForegroundColor Green
Write-Host "Set the following environment variables for OpenAI:" -ForegroundColor Gray
Write-Host '$env:OPENAI_API_KEY = "sk-your-openai-api-key-here"' -ForegroundColor White
Write-Host '$env:OPENAI_MODEL = "gpt-4"  # Optional, defaults to gpt-4' -ForegroundColor Gray
Write-Host '$env:OPENAI_ORGANIZATION = "org-your-organization-id"  # Optional' -ForegroundColor Gray
Write-Host ""

# Example (DO NOT USE IN PRODUCTION):
# $env:OPENAI_API_KEY = "sk-example-key-replace-with-real-key"
# $env:OPENAI_MODEL = "gpt-4"

# Azure OpenAI Configuration
Write-Host "☁️  Azure OpenAI Configuration:" -ForegroundColor Green
Write-Host "Set the following environment variables for Azure OpenAI:" -ForegroundColor Gray
Write-Host '$env:AZURE_OPENAI_API_KEY = "your-azure-openai-key"' -ForegroundColor White
Write-Host '$env:AZURE_OPENAI_ENDPOINT = "https://your-resource.openai.azure.com/"' -ForegroundColor White
Write-Host '$env:AZURE_OPENAI_DEPLOYMENT_NAME = "your-deployment-name"' -ForegroundColor White
Write-Host '$env:AZURE_OPENAI_API_VERSION = "2024-02-01"  # Optional' -ForegroundColor Gray
Write-Host ""

# Example (DO NOT USE IN PRODUCTION):
# $env:AZURE_OPENAI_API_KEY = "example-key-replace-with-real-key"
# $env:AZURE_OPENAI_ENDPOINT = "https://example.openai.azure.com/"
# $env:AZURE_OPENAI_DEPLOYMENT_NAME = "gpt-4"

# Claude Configuration
Write-Host "🧠 Claude Configuration:" -ForegroundColor Green
Write-Host "Set the following environment variables for Claude:" -ForegroundColor Gray
Write-Host '$env:CLAUDE_API_KEY = "your-claude-api-key"' -ForegroundColor White
Write-Host '$env:CLAUDE_MODEL = "claude-3-sonnet-20240229"  # Optional' -ForegroundColor Gray
Write-Host ""

# Example (DO NOT USE IN PRODUCTION):
# $env:CLAUDE_API_KEY = "example-key-replace-with-real-key"
# $env:CLAUDE_MODEL = "claude-3-sonnet-20240229"

# Llama Configuration
Write-Host "🦙 Llama Configuration:" -ForegroundColor Green
Write-Host "Set the following environment variables for Llama:" -ForegroundColor Gray
Write-Host '$env:LLAMA_API_URL = "https://your-llama-endpoint.com"' -ForegroundColor White
Write-Host '$env:LLAMA_API_KEY = "your-api-key"  # Optional, depends on your setup' -ForegroundColor Gray
Write-Host '$env:LLAMA_MODEL = "llama2-70b-chat"  # Optional' -ForegroundColor Gray
Write-Host ""

# Example (DO NOT USE IN PRODUCTION):
# $env:LLAMA_API_URL = "https://api.together.xyz"
# $env:LLAMA_API_KEY = "example-key-replace-with-real-key"
# $env:LLAMA_MODEL = "llama2-70b-chat"

# Gemini Configuration
Write-Host "🔮 Google Gemini Configuration:" -ForegroundColor Green
Write-Host "Set the following environment variables for Gemini:" -ForegroundColor Gray
Write-Host '$env:GEMINI_API_KEY = "your-gemini-api-key"' -ForegroundColor White
Write-Host '$env:GEMINI_MODEL = "gemini-pro"  # Optional, defaults to gemini-pro' -ForegroundColor Gray
Write-Host ""

# Example (DO NOT USE IN PRODUCTION):
# $env:GEMINI_API_KEY = "example-key-replace-with-real-key"
# $env:GEMINI_MODEL = "gemini-pro"

# DeepSeek Configuration
Write-Host "🔍 DeepSeek Configuration:" -ForegroundColor Green
Write-Host "Set the following environment variables for DeepSeek:" -ForegroundColor Gray
Write-Host '$env:DEEPSEEK_API_KEY = "your-deepseek-api-key"' -ForegroundColor White
Write-Host '$env:DEEPSEEK_API_URL = "https://api.deepseek.com/v1"  # Optional' -ForegroundColor Gray
Write-Host '$env:DEEPSEEK_MODEL = "deepseek-chat"  # Optional' -ForegroundColor Gray
Write-Host ""

# Example (DO NOT USE IN PRODUCTION):
# $env:DEEPSEEK_API_KEY = "example-key-replace-with-real-key"
# $env:DEEPSEEK_MODEL = "deepseek-coder"

Write-Host "📋 Quick Setup Instructions:" -ForegroundColor Cyan
Write-Host "1. Copy this file: Copy-Item setup-env.example.ps1 setup-env.local.ps1" -ForegroundColor Gray
Write-Host "2. Edit the local file with your real API keys" -ForegroundColor Gray
Write-Host "3. Run: . ./setup-env.local.ps1" -ForegroundColor Gray
Write-Host "4. Build and test: ./build.ps1 -Clean -Import -Test" -ForegroundColor Gray
Write-Host ""

Write-Host "🔒 Security Notes:" -ForegroundColor Yellow
Write-Host "- Never commit API keys to version control" -ForegroundColor Gray
Write-Host "- Add setup-env.local.ps1 to your .gitignore (already included)" -ForegroundColor Gray
Write-Host "- Consider using Azure Key Vault or similar for production deployments" -ForegroundColor Gray
Write-Host "- Rotate your API keys regularly" -ForegroundColor Gray
Write-Host ""

Write-Host "🧪 Testing Your Setup:" -ForegroundColor Cyan
Write-Host "After setting up environment variables, test with:" -ForegroundColor Gray
Write-Host 'cmd-assist "show current directory contents"' -ForegroundColor White
Write-Host ""

# Function to check if environment variables are set
function Test-CmdAssistEnvironment {
    Write-Host "🔍 Checking Environment Configuration:" -ForegroundColor Cyan
    
    $providers = @{
        "OpenAI" = @("OPENAI_API_KEY")
        "Azure OpenAI" = @("AZURE_OPENAI_API_KEY", "AZURE_OPENAI_ENDPOINT", "AZURE_OPENAI_DEPLOYMENT_NAME")
        "Claude" = @("CLAUDE_API_KEY")
        "Llama" = @("LLAMA_API_URL")
        "Gemini" = @("GEMINI_API_KEY")
        "DeepSeek" = @("DEEPSEEK_API_KEY")
    }
    
    foreach ($provider in $providers.Keys) {
        Write-Host "  $provider : " -NoNewline -ForegroundColor Gray
        
        $allSet = $true
        $missingVars = @()
        
        foreach ($var in $providers[$provider]) {
            if (-not (Test-Path "env:$var") -or -not $env:$var) {
                $allSet = $false
                $missingVars += $var
            }
        }
        
        if ($allSet) {
            Write-Host "✅ Configured" -ForegroundColor Green
        } else {
            Write-Host "❌ Missing: $($missingVars -join ', ')" -ForegroundColor Red
        }
    }
}

# Export the test function
Export-ModuleMember -Function Test-CmdAssistEnvironment

Write-Host "💡 Tip: Run 'Test-CmdAssistEnvironment' to check your configuration" -ForegroundColor Cyan