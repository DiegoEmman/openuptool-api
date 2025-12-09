$ErrorActionPreference = 'Stop'
$baseUrl = 'http://localhost:5000'
$loginBody = '{"email":"admin@openuptool.com","password":"Password123!"}'
$login = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType 'application/json'
$token = $login.token
Write-Host "Admin token OK (len=$($token.Length))"
$headers = @{ Authorization = "Bearer $token"; 'Content-Type'='application/json' }
$projects = Invoke-RestMethod -Uri "$baseUrl/api/projects" -Method GET -Headers $headers
$projectId = $projects[0].id
Write-Host "ProjectId: $projectId"
$members = Invoke-RestMethod -Uri "$baseUrl/api/projects/$projectId/members" -Method GET -Headers $headers
$member = $members | Where-Object { $_.userEmail -eq 'viewer@openuptool.com' }
if (-not $member) { Write-Host 'Member viewer@openuptool.com not found'; exit 1 }
$memberId = $member.id
Write-Host "MemberId: $memberId"
$viewerRole = (Invoke-RestMethod -Uri "$baseUrl/api/roles" -Method GET -Headers $headers | Where-Object { $_.name -eq 'Viewer' }).id
Write-Host "ViewerRole: $viewerRole"
$putUrl = "$baseUrl/api/projects/$projectId/members/$memberId/role"
$putBody = '{"roleId":"' + $viewerRole + '"}'
Write-Host "PUT $putUrl`nBody: $putBody`n"
try {
    $response = Invoke-WebRequest -Uri $putUrl -Method Put -Headers $headers -Body $putBody -UseBasicParsing -ErrorAction Stop
    Write-Host "Status: $($response.StatusCode)"
    Write-Host $response.Content
} catch {
    Write-Host "PUT failed: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $sr = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $body = $sr.ReadToEnd()
        Write-Host "Response body:`n$body"
    }
}
