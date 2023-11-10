using System.Net;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc;

namespace WakeOnLANServer;

[ApiController]
[Route("/")]
public class APIController : ControllerBase
{
    private readonly DevicesInstance _Device;
    public APIController(DevicesInstance Device)
    {
        _Device = Device;
    }

    [HttpPost]
    public async Task<StatusCodeResult> Post(int? VLAN)
    {
        if (VLAN == null || VLAN < 0 || VLAN > 4096)
        {
            VLAN = 0;
        }
        List<PhysicalAddress> MACList;
        string body = "";
        if (Request.HasFormContentType && Request.Form["text"].Any())
        {
            var text = Request.Form["text"].First();
            if (text != null)
            {
                body = await new StringReader(text).ReadToEndAsync();
            }
        }
        else
        {
            using var sr = new StreamReader(HttpContext.Request.Body);
            body = await sr.ReadToEndAsync();
        }
        MACList = WakeOnLAN.Parse(body);
        foreach (var device in _Device.Devices)
        {
            foreach (var MAC in MACList)
            {
                try
                {
                    WakeOnLAN.Send(device, MAC, (ushort)VLAN);
                }
                catch (SharpPcap.PcapException)
                {
                    _Device.Dispose();
                    _Device.OpenDevices();
                    return StatusCode((int)HttpStatusCode.InternalServerError);
                }
            }
        }
        return StatusCode((int)HttpStatusCode.OK);
    }
}