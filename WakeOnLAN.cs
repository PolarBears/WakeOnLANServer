using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using PacketDotNet;
using PacketDotNet.Utils;
using SharpPcap;

namespace WakeOnLANServer;

public class WakeOnLAN
{
    public static void Send(ILiveDevice device, PhysicalAddress DstMAC, ushort VLAN = 0)
    {
        EthernetPacket ethernetPacket;
        Ieee8021QPacket ieee8021QPacket;
        WakeOnLanPacket wakeOnLanPacket = new WakeOnLanPacket(DstMAC);

        if (VLAN == 0)
        {
            ethernetPacket = new EthernetPacket(device.MacAddress,
                                                BroadcastMACAddress,
                                                EthernetType.WakeOnLan)
            {
                PayloadPacket = wakeOnLanPacket
            };
        }
        else
        {
            // 如果使用WakeOnLanPacket.BytesSegment进行构造，会造成WakeOnLanPacket头4个byte被修改
            ieee8021QPacket = new Ieee8021QPacket(new ByteArraySegment(new byte[4]))
            {
                VlanIdentifier = VLAN,
                Type = EthernetType.WakeOnLan
            };

            ethernetPacket = new EthernetPacket(device.MacAddress,
                                                BroadcastMACAddress,
                                                EthernetType.VLanTaggedFrame);
            ieee8021QPacket.PayloadPacket = wakeOnLanPacket;
            ethernetPacket.PayloadPacket = ieee8021QPacket;
        }
        device.SendPacket(ethernetPacket);
    }
    /// <summary>
    /// 00:00:00:00:00:00
    /// </summary> 
    private static PhysicalAddress NoneMACAddress = new PhysicalAddress(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
    /// <summary>
    /// ff:ff:ff:ff:ff:ff
    /// </summary>
    private static PhysicalAddress BroadcastMACAddress = new PhysicalAddress(new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
    /// <summary>
    /// 匹配 01:23:45:67:89:ab 或 01-23-45-67-89-ab 格式的MAC地址
    /// </summary> 
    private static Regex Regex_A = new Regex(@"(?:^|\s)((?:[a-fA-F0-9]{1,2}[:-]){5}[?:a-fA-F0-9]{1,2})(?:$|\s|\(|\[)");
    /// <summary>
    /// 匹配 0123.4567.89ab 或 0123-4567-89ab 格式的MAC地址
    /// </summary>
    private static Regex Regex_B = new Regex(@"(?:^|\s)((?:[a-fA-F0-9]{4}[\.-]){2}[a-fA-F0-9]{4})(?:$|\s|\(|\[)");
    /// <summary>
    /// 匹配 0123456789ab 格式的MAC地址
    /// </summary> 
    private static Regex Regex_C = new Regex(@"(?:^|\s)([a-fA-F0-9]{12})(?:$|\s|\(|\[)");

    /// <summary>
    /// 返回所有匹配到的MAC地址
    /// </summary>
    /// <param name="MACText"></param>
    /// <returns></returns> <summary>
    /// 需要进行匹配的文本
    /// </summary>
    /// <param name="MACText"></param>
    /// <returns></returns>
    public static List<PhysicalAddress> Parse(string MACText)
    {
        List<PhysicalAddress> addrList = new List<PhysicalAddress>();
        List<string> tmp = new List<string>();

        tmp.AddRange((from item in Regex_A.Matches(MACText)
                      from _ in item.Groups.Values
                      where _.Length == 17
                      select _.Value).ToList());

        tmp.AddRange((from item in Regex_B.Matches(MACText)
                      from _ in item.Groups.Values
                      where _.Length == 14
                      select _.Value.Replace('-', '.')).ToList());

        tmp.AddRange((from item in Regex_C.Matches(MACText)
                      from _ in item.Groups.Values
                      where _.Length == 12
                      select _.Value).ToList());

        foreach (var item in tmp)
        {
            if (PhysicalAddress.TryParse(item, out PhysicalAddress? addr))
            {
                addrList.Add(addr);
            }
        }

        addrList = addrList.Distinct().ToList();
        addrList.Remove(NoneMACAddress);
        addrList.Remove(BroadcastMACAddress);
        return addrList;
    }
}