#Requires -Version 3.0

Param(
	[string] [Parameter(Mandatory=$true)] $Environment,
	[string] [Parameter(Mandatory=$true)] $SubscriptionName,
	[bool] [Parameter(Mandatory=$false)] $DeployCredentials = $true,
	[bool] [Parameter(Mandatory=$false)] $DeployArm = $true,
	[bool] [Parameter(Mandatory=$false)] $UpgradeDatabase = $true
)

. "$PSScriptRoot\Deploy-Credentials.ps1"
. "$PSScriptRoot\Upgrade-Database.ps1"
. "$PSScriptRoot\Utils.ps1"

try {
    [Microsoft.Azure.Common.Authentication.AzureSession]::ClientFactory.AddUserAgent("VSAzureTools-$UI$($host.name)".replace(' ','_'), '3.0.0')
} catch { }

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3
$OptionalParameters = New-Object -TypeName Hashtable

Write-Status "Selecting Azure subscription... "
try {
	Select-AzureRmSubscription -SubscriptionName  $SubscriptionName
}
catch{
	Login-AzureRmAccount;
	Select-AzureRmSubscription -SubscriptionName  $SubscriptionName
}
Write-Succeed

$ResourceGroupName = "BurnForMoney_$Environment";
$KeyVaultName = "burnformoneykv" + $Environment.ToLower();
$ResourceGroupLocation= 'West Europe';

$TemplateFileName = "Template.json";
if ($Environment -eq "Dev")
{
	$TemplateFileName = "Template.Dev.json";
}


$TemplateFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, $TemplateFileName))
$TemplateParametersFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, "Template.$Environment.parameters.json"))

New-AzureRmResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation -Verbose -Force

if ($DeployCredentials)
{
	$StravaKeyVaultName = "bfmkvstravatokens" + $Environment.ToLower();
	CreateKeyVault -Environment $Environment `
					-ResourceGroupName $ResourceGroupName `
					-ResourceGroupLocation $ResourceGroupLocation `
					-KeyVaultName $StravaKeyVaultName

	DeployCredentials -Environment $Environment `
		-ResourceGroupName $ResourceGroupName `
		-ResourceGroupLocation $ResourceGroupLocation `
		-KeyVaultName $KeyVaultName
}

$ErrorMessages = $null

if ($DeployArm)
{
	Write-Status "Processing a new group deployment... "
	New-AzureRmResourceGroupDeployment -Name ((Get-ChildItem $TemplateFile).BaseName + '-' + ((Get-Date).ToUniversalTime()).ToString('MMdd-HHmm')) `
                                    -ResourceGroupName $ResourceGroupName `
                                    -TemplateFile $TemplateFile `
                                    -TemplateParameterFile $TemplateParametersFile `
                                    @OptionalParameters `
                                    -Force -Verbose `
                                    -ErrorVariable ErrorMessages

	if ($ErrorMessages)
	{
		Write-Fail
		Write-Output '', 'Template deployment returned the following errors:', @(@($ErrorMessages) | ForEach-Object { $_.Exception.Message.TrimEnd("`r`n") })
	} 
	else 
	{
		Write-Succeed
	}
}

if ($UpgradeDatabase -And (-Not $ErrorMessages))
{
	$connectionStringSecret = Get-AzureKeyVaultSecret -VaultName $KeyVaultName -Name "ConnectionStrings--Sql"
	Upgrade-Database -ConnectionString $connectionStringSecret.SecretValueText -ScriptsPath "$PSScriptRoot\SqlScripts\"
}

Read-Host "Press ENTER to continue"