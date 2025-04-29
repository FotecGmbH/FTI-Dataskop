#
# Skript für BETA Nuget
# Wird in Continuous Deployment in Azure DevOps verwendet (für BETA Branch)
#
# (C) FOTEC Forschungs- und Technologietransfer GmbH
# (C) 2018-2020 Michael Kollegger
#

$csprojPath = $args[0] # path to the file i.e. 'C:\Code\Fotec\BISS\BISS.Core\Biss.Core\Biss.Core.csproj'
Write-Host "Starting process of generating new version number for the csproj"

$buildGitId = ($env:BUILD_SOURCEVERSION)
Write-Host "GitId:" + $buildGitId
$buildGitId = $buildGitId.Substring(0,8)
Write-Host "GitIdFirst8:" + $buildGitId
Write-Host $buildGitId

if ($csprojPath -eq $null)
{
    Write-Error "No .Net Core csproj defined!"
    exit 1
}

$xml=New-Object XML
$xml.Load($csprojPath)
if ($xml -eq $null)
{
    Write-Error "No XML SLN found!"
    exit 1
}

$PropertyGroupNodes = $xml.Project.PropertyGroup
foreach($pg in $PropertyGroupNodes)
{
    $versionNode = $pg.Version
    if ($versionNode -notlike $null) 
    {
		$datenow = (Get-Date).ToString("yyyyMMdd")

        $versionNode = $versionNode + "-beta" + "-" + $datenow + "-" + $buildGitId
        $updateData = "##vso[task.setvariable variable=biss.build]" + $versionNode
        Write-Host $updateData
        Write-Host "Set biss.build to"$versionNode
        exit 0
    }
}
Write-Error "No Project.PropertyGroup.Version found!"
exit 1



