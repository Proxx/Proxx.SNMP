Function Invoke-SnmpGet {
	<#
		.SYNOPSIS
			Invoke SnmpGet returnes a object with the results from snmp.

		.DESCRIPTION
			Invoke SnmpGet returnes a object with the results from snmp.

		.PARAMETER  IpAddress
			This parameter contains the node adress of the agent.

		.PARAMETER  OID
			this variable contains the OID (Object IDentifier)

		.EXAMPLE
			PS C:\> Invoke-SnmpGet -IpAddress 192.168.1.100 -OID  1.3.6.1.2.1.1.1.0 | ft -AutoSize

			OID               Type        Value                                                                                  
			---               ----        -----                                                                                  
			1.3.6.1.2.1.1.1.0 OctetString HP ETHERNET

		.EXAMPLE
			PS C:\> "192.168.1.100", "192.168.1.104", "192.168.1.94"| Invoke-SnmpGet -OID  1.3.6.1.2.1.1.1.0 | ft -AutoSize

			Node		  OID               Type        Value                                                                                  
			----		  ---               ----        -----                                                                                  
			192.168.1.100 1.3.6.1.2.1.1.1.0 OctetString HP ETHERNET
			192.168.1.104 1.3.6.1.2.1.1.1.0 OctetString SonicWALL NSA
			192.168.1.94  1.3.6.1.2.1.1.1.0 OctetString Digi Connect                    


		.INPUTS
			System.String

		.LINK
			http://www.proxx.nl

	#>
	[CmdletBinding()]
    [OutputType([System.Object])]
	Param(
		[String]$Community = "public",
		[Parameter(
			Mandatory=$true,
			Position=0,
			ValueFromPipeline=$true,
			ValueFromPipelineByPropertyName=$true
		)]
		[Alias("Address","ComputerName","IP","Node")]
		[String[]]$IpAddress=$null,
		[Parameter(Mandatory=$true, Position=1)]
		[String[]]$OID=$null,
		[int] $Port=161,
		[int] $Retry =  1,
		[int] $TimeOut = 2000,
		[ValidateSet("1","2")]
		[String]$Version="2"
		
	)
	
	Begin { 
		$SimpleSnmp = New-Object -TypeName SnmpSharpNet.SimpleSnmp
		$SimpleSnmp.Community = $Community
		$SimpleSnmp.Retry = $Retry
		$SimpleSnmp.PeerPort = $Port
		$SimpleSnmp.Timeout = $TimeOut
		[SnmpSharpNet.SnmpVersion]$ver = [SnmpSharpNet.SnmpVersion]::Ver2
		Switch($Version) {
			1 {$Ver = [SnmpSharpNet.SnmpVersion]::Ver1 }
			2 {$Ver = [SnmpSharpNet.SnmpVersion]::Ver2 }
			default {$Ver = [SnmpSharpNet.SnmpVersion]::Ver2 }
		}
	}
	Process {
		ForEach($Node in $IpAddress)  {
			$SimpleSnmp.PeerIP = $Node
			ForEach($x in $OID) {
				$Response = $SimpleSnmp.Get($Ver,$x)
				if ($Response) {
					if ($Response.Count -gt 0) {
						foreach ($var in $Response.GetEnumerator()) {
							Write-Output -InputObject ([PSCustomObject] @{
								Node = $Node
								OID = $var.Key.ToString()
								Type = [snmpsharpnet.SnmpConstants]::GetTypeName($var.Value.Type)
								Value = $var.Value.ToString()
							})
						}
					}
				} Else { Write-Warning -Message "$Node returned Null" }
			}
		}
	}
}
