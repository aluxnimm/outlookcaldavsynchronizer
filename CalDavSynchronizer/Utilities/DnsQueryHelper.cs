using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Utilities
{
  class DnsQueryHelper
  {
    private const Int32 DNS_ERROR_RCODE_NAME_ERROR = 9003;
    private const Int32 DNS_INFO_NO_RECORDS = 9501;

    [DllImport("dnsapi", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
    private static extern int DnsQuery([MarshalAs(UnmanagedType.VBByRefStr)]ref string pszName, QueryTypes wType, QueryOptions options, int aipServers, ref IntPtr ppQueryResults, int pReserved);

    [DllImport("dnsapi", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern void DnsRecordListFree(IntPtr pRecordList, int FreeType);


    public static List<string> GetSRVRecordList (string lookupString)
    {
      var queryResultsSet = IntPtr.Zero;
      var records = new List<String>();

      try
      {
        int dnsStatus = DnsQuery (
          ref lookupString, 
          QueryTypes.DNS_TYPE_SRV, 
          QueryOptions.DNS_QUERY_STANDARD, 
          0, 
          ref queryResultsSet, 
          0
        );

        if (dnsStatus == DNS_ERROR_RCODE_NAME_ERROR || dnsStatus == DNS_INFO_NO_RECORDS)
          return records;
        if (dnsStatus != 0)
          throw new Win32Exception (dnsStatus);

        SRVRecord recSRV;

        for (var pointer = queryResultsSet; pointer !=IntPtr.Zero; pointer = recSRV.pNext)
        {
          recSRV = (SRVRecord)Marshal.PtrToStructure (pointer, typeof (SRVRecord));
          if (recSRV.wType == (ushort)QueryTypes.DNS_TYPE_SRV)
          {
            string target = Marshal.PtrToStringAuto (recSRV.pNameTarget);
            target += ":" + recSRV.wPort;
            records.Add (target);
          }
        }
      }
      finally
      {
        const Int32 dnsFreeRecordList = 1;
        if (queryResultsSet != IntPtr.Zero)
          DnsRecordListFree (queryResultsSet, dnsFreeRecordList);
      }
      return records;
    }

    public static string GetTxtRecord (string lookupString)
    {
      var queryResultsSet = IntPtr.Zero;

      try
      {
        var dnsStatus = DnsQuery (
          ref lookupString,
          QueryTypes.DNS_TYPE_TEXT, 
          QueryOptions.DNS_QUERY_STANDARD, 
          0,
          ref queryResultsSet,
          0
        );

        if (dnsStatus == DNS_ERROR_RCODE_NAME_ERROR || dnsStatus == DNS_INFO_NO_RECORDS)
          return null;
        if (dnsStatus != 0)
          throw new Win32Exception (dnsStatus);

        TXTRecord dnsRecord;
        for (var pointer = queryResultsSet; pointer != IntPtr.Zero; pointer = dnsRecord.pNext)
        {
          dnsRecord = (TXTRecord)Marshal.PtrToStructure (pointer, typeof (TXTRecord));
          if (dnsRecord.wType == (ushort)QueryTypes.DNS_TYPE_TEXT)
          {
            var lines = new List<String>();
            var stringArrayPointer = pointer
              + Marshal.OffsetOf (typeof (TXTRecord), "pStringArray").ToInt32();

            for (var i = 0; i < dnsRecord.dwStringCount; ++i)
            {
              var stringPointer = (IntPtr)Marshal.PtrToStructure (stringArrayPointer, typeof (IntPtr));
              lines.Add (Marshal.PtrToStringUni (stringPointer));
              stringArrayPointer += IntPtr.Size;
            }
            return String.Join (Environment.NewLine, lines);
          }
        }
        return null;
      }
      finally
      {
        const Int32 dnsFreeRecordList = 1;
        if (queryResultsSet != IntPtr.Zero)
          DnsRecordListFree (queryResultsSet, dnsFreeRecordList);
      }
    }

    private enum QueryOptions
    {
      DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE = 1,
      DNS_QUERY_BYPASS_CACHE = 8,
      DNS_QUERY_DONT_RESET_TTL_VALUES = 0x100000,
      DNS_QUERY_NO_HOSTS_FILE = 0x40,
      DNS_QUERY_NO_LOCAL_NAME = 0x20,
      DNS_QUERY_NO_NETBT = 0x80,
      DNS_QUERY_NO_RECURSION = 4,
      DNS_QUERY_NO_WIRE_QUERY = 0x10,
      DNS_QUERY_RESERVED = -16777216,
      DNS_QUERY_RETURN_MESSAGE = 0x200,
      DNS_QUERY_STANDARD = 0,
      DNS_QUERY_TREAT_AS_FQDN = 0x1000,
      DNS_QUERY_USE_TCP_ONLY = 2,
      DNS_QUERY_WIRE_ONLY = 0x100
    }

    private enum QueryTypes
    {
      DNS_TYPE_SRV = 0x0021,
      DNS_TYPE_TEXT = 0x0010
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct SRVRecord
    {
      public IntPtr pNext;
      public string pName;
      public ushort wType;
      public ushort wDataLength;
      public int flags;
      public int dwTtl;
      public int dwReserved;
      public IntPtr pNameTarget;
      public ushort wPriority;
      public ushort wWeight;
      public ushort wPort;
      public ushort Pad;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct TXTRecord
    {
      public IntPtr pNext;
      public String pName;
      public Int16 wType;
      public Int16 wDataLength;
      public Int32 flags;
      public Int32 dwTtl;
      public Int32 dwReserved;
      public Int32 dwStringCount;
      public String pStringArray;
    }
  }
}
