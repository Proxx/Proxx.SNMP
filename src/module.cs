using System;
using System.Collections.Generic;
using System.Management.Automation;
using SnmpSharpNet;
using System.Net;

namespace Proxx.SNMP
{
    [OutputType("PSObject")]
    [Cmdlet(VerbsLifecycle.Invoke, "SnmpGet", SupportsShouldProcess = true)]
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
        [Parameter(Mandatory = false)]
        public string Community
        {
            get { return _Community; }
            set { _Community = value; }
        }
        [Parameter(Mandatory = false)]
        public int Port
        {
            get { return _Port; }
            set { _Port = value; }
        }
        [Parameter(Mandatory = false)]
        public int Retry
        {
            get { return _Retry; }
            set { _Retry = value; }
        }
        [Parameter(Mandatory = false)]
        public int TimeOut
        {
            get { return _TimeOut; }
            set { _TimeOut = value; }
        }
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
        }
        #endregion
    }

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
        [Parameter(Mandatory = false)]
        public string Community
        {
            get { return _Community; }
            set { _Community = value; }
        }
        [Parameter(Mandatory = false)]
        public int Port
        {
            get { return _Port; }
            set { _Port = value; }
        }
        [Parameter(Mandatory = false)]
        public int Retry
        {
            get { return _Retry; }
            set { _Retry = value; }
        }
        [Parameter(Mandatory = false)]
        public int TimeOut
        {
            get { return _TimeOut; }
            set { _TimeOut = value; }
        }
        [Parameter(Mandatory = false)]
        public SnmpVersion Version
        {
            get { return _Version; }
            set { _Version = value; }
        }

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
                {
                    ThrowTerminatingError(new ErrorRecord(new Exception("Not a valid ipaddress"), "", ErrorCategory.InvalidData, ""));
                    //ip = System.Net.Dns.GetHostEntry(node).AddressList[0];
                    //_SimpleSnmp.PeerIP = ip;
                }
                else
                {
                    _SimpleSnmp.PeerIP = ip;
                }
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
                                //PSNoteProperties are not strongly typed but do contain an explicit type.
                                obj.Properties.Add(new PSNoteProperty("Node", node));
                                obj.Properties.Add(new PSNoteProperty("OID", item.Key.ToString()));
                                obj.Properties.Add(new PSNoteProperty("Type", SnmpConstants.GetTypeName(item.Value.Type)));
                                obj.Properties.Add(new PSNoteProperty("Value", item.Value.ToString()));
                                if (_Force)
                                {
                                    LastOid = item.Key.ToString();
                                }
                                else
                                {
                                    if (_RootOID.IsRootOf(item.Key)) { LastOid = item.Key.ToString(); }
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
                    else { Console.WriteLine("OID " + LastOid + " returned Null "); LastOid = null; }
                
                }

            }
        }
        #endregion
    }
}

