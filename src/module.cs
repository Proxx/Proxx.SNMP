using System;
using System.Collections.Generic;
using System.Management.Automation;
using SnmpSharpNet;
using System.Net;

namespace Proxx.SNMP
{
    /// <list type="alertSet">
    ///   <item>
    ///     <term>Proxx.SNMP</term>
    ///     <description>
    ///     Author: Marco van G. (Proxx)
    ///     Website: www.Proxx.nl
    ///     </description>
    ///   </item>
    /// </list>
    [OutputType("PSObject")]
    [Cmdlet(VerbsLifecycle.Invoke, "SnmpGet", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low)]
    public class InvokeSnmpGet : PSCmdlet
    {
        #region Variables
        private string[] _IpAddress;
        private string[] _Oid;
        private string _Community = "public";
        private int _Port=161;
        private int _Retry=1;
        private int _TimeOut=2000;
        private SnmpVersion _Version = SnmpVersion.Ver2;
        private SimpleSnmp _SimpleSnmp;
        #endregion

        #region Parameters
        /// <summary>
        /// <para type="description">Specifies the address of the device.</para>
        /// </summary>
        [Alias("Address", "ComputerName", "IP", "Node")]
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true
        )]
        public string[] IpAddress
        {
            get { return _IpAddress; }
            set { _IpAddress = value; }
        }
        /// <summary>
        /// <para type="description">Specifies the object identifier (OID). One or more object identifiers (OIDs) may be given as arguments.</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = 2,
            ValueFromPipelineByPropertyName = true
        )]
        public string[] Oid
        {
            get { return _Oid; }
            set { _Oid = value; }
        }
        /// <summary>
        /// <para type="description">Specifies community string for SNMP communication. The default is "public".</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Community
        {
            get { return _Community; }
            set { _Community = value; }
        }
        /// <summary>
        /// <para type="description">Specifies port used for SNMP communication. The default is "161".</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int Port
        {
            get { return _Port; }
            set { _Port = value; }
        }
        /// <summary>
        /// <para type="description">Specifies ammount of times SNMP will retry to get values. The default is 1.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int Retry
        {
            get { return _Retry; }
            set { _Retry = value; }
        }
        /// <summary>
        /// <para type="description">Determines how long Windows PowerShell waits for this command to complete. The default is 2seconds.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int TimeOut
        {
            get { return _TimeOut; }
            set { _TimeOut = value; }
        }
        /// <summary>
        /// <para type="description">Specifies the SNMP Version used for the connection. The default is 2.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SnmpVersion Version
        {
            get { return _Version; }
            set { _Version = value; }
        }
        #endregion

        #region Runtime
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _SimpleSnmp = new SimpleSnmp();
            _SimpleSnmp.Community = _Community;
            _SimpleSnmp.Retry = _Retry;
            _SimpleSnmp.Timeout = _TimeOut;
        }


        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            foreach(string node in _IpAddress)
            {
                _SimpleSnmp.PeerIP = System.Net.IPAddress.Parse(node);
                Dictionary<Oid, AsnType> response = _SimpleSnmp.Get(_Version, _Oid);
                if (response != null)
                {
                    foreach (KeyValuePair<Oid, AsnType> item in response)
                    {
                        if (!item.Key.ToString().Equals("0.0"))
                        {
                            PSObject obj = new PSObject();
                            //PSNoteProperties are not strongly typed but do contain an explicit type.
                            obj.Properties.Add(new PSNoteProperty("Node", node));
                            obj.Properties.Add(new PSNoteProperty("OID", item.Key.ToString()));
                            obj.Properties.Add(new PSNoteProperty("Type", SnmpConstants.GetTypeName(item.Value.Type)));
                            obj.Properties.Add(new PSNoteProperty("Value", item.Value.ToString()));
                            WriteObject(obj);
                        }
                    }
                }
                else { WriteError(new ErrorRecord(new Exception("OID " + string.Join(", ", _Oid) + " returned Null "),"", ErrorCategory.ProtocolError,null)); }

            }
        }
        #endregion
    }
    /// <list type="alertSet">
    ///   <item>
    ///     <term>Proxx.SNMP</term>
    ///     <description>
    ///     Author: Marco van G. (Proxx)
    ///     Website: www.Proxx.nl
    ///     </description>
    ///   </item>
    /// </list>
    [OutputType("PSObject")]
    [Cmdlet(VerbsLifecycle.Invoke, "SnmpWalk", SupportsShouldProcess = true)]
    public class InvokeSnmpWalk : PSCmdlet
    {
        #region Variables
        private string[] _IpAddress;
        private string _Oid = "1.3.6.1";
        private string _Community = "public";
        private bool _Force;
        private int _Port = 161;
        private int _Retry = 1;
        private int _TimeOut = 2000;
        private SnmpVersion _Version = SnmpVersion.Ver2;
        private SimpleSnmp _SimpleSnmp;
        private Oid _RootOID;
        #endregion

        #region Parameters
        /// <summary>
        /// <para type="description">Specifies the address of the device.</para>
        /// </summary>
        [Alias("Address", "ComputerName", "IP", "Node")]
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true
        )]
        public string[] IpAddress
        {
            get { return _IpAddress; }
            set { _IpAddress = value; }
        }
        /// <summary>
        /// <para type="description">Specifies the object identifier (OID). One or more object identifiers (OIDs) may be given as arguments.</para>
        /// </summary>
        [Parameter(
            Mandatory = false,
            Position = 2,
            ValueFromPipelineByPropertyName = true
        )]
        public string Oid
        {
            get { return _Oid; }
            set { _Oid = value; }
        }
        /// <summary>
        /// <para type="description">Specifies community string for SNMP communication. The default is "public".</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Community
        {
            get { return _Community; }
            set { _Community = value; }
        }
        /// <summary>
        /// <para type="description">Specifies port used for SNMP communication. The default is "161".</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int Port
        {
            get { return _Port; }
            set { _Port = value; }
        }
        /// <summary>
        /// <para type="description">Specifies ammount of times SNMP will retry to get values. The default is 1.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int Retry
        {
            get { return _Retry; }
            set { _Retry = value; }
        }
        /// <summary>
        /// <para type="description">Determines how long Windows PowerShell waits for this command to complete. The default is 2seconds.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int TimeOut
        {
            get { return _TimeOut; }
            set { _TimeOut = value; }
        }
        /// <summary>
        /// <para type="description">Specifies the SNMP Version used for the connection. The default is 2.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SnmpVersion Version
        {
            get { return _Version; }
            set { _Version = value; }
        }
        /// <summary>
        /// <para type="description">returns the next value even if Current OID is not Root of Parameter OID</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force
        {
            get { return _Force; }
            set { _Force = value; }
        }
        #endregion

        #region Runtime
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _SimpleSnmp = new SimpleSnmp();
            _SimpleSnmp.Community = _Community;
            _SimpleSnmp.Retry = _Retry;
            _SimpleSnmp.Timeout = _TimeOut;
            if (_Force) { _RootOID = null; }
            else { _RootOID = new Oid(_Oid.ToString()); }
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            foreach (string node in _IpAddress)
            {
                IPAddress ip = null;
                if (!System.Net.IPAddress.TryParse(node, out ip))
                    ThrowTerminatingError(new ErrorRecord(new Exception("Not a valid ipaddress"), "", ErrorCategory.InvalidData, ""));
                else
                    _SimpleSnmp.PeerIP = ip;
                _SimpleSnmp.PeerIP = System.Net.IPAddress.Parse(ip.ToString());
                string LastOid = _Oid;
                while(LastOid != null)
                {
                    Dictionary<Oid, AsnType> response = _SimpleSnmp.GetNext(_Version, new string[]{ LastOid });
                    if (response != null)
                    {
                        foreach (KeyValuePair<Oid, AsnType> item in response)
                        {
                            if (!item.Key.ToString().Equals("0.0"))
                            {
                                PSObject obj = new PSObject();
                                obj.Properties.Add(new PSNoteProperty("Node", node));
                                obj.Properties.Add(new PSNoteProperty("OID", item.Key.ToString()));
                                obj.Properties.Add(new PSNoteProperty("Type", SnmpConstants.GetTypeName(item.Value.Type)));
                                obj.Properties.Add(new PSNoteProperty("Value", item.Value.ToString()));
                                if (_Force)
                                    LastOid = item.Key.ToString();
                                else
                                {
                                    if (_RootOID.IsRootOf(item.Key))
                                        LastOid = item.Key.ToString();
                                    else
                                    {
                                        LastOid = null;
                                        break;
                                    }
                                }
                                WriteObject(obj);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("OID " + LastOid + " returned Null ");
                        LastOid = null;
                    }
                }
            }
        }
        #endregion
    }
}

