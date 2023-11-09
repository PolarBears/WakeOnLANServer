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
    public async Task<IActionResult> Post(int? VLAN)
    {
        using var sr = new StreamReader(HttpContext.Request.Body);
        var postData = await sr.ReadToEndAsync();
        var MACList = WakeOnLAN.Parse(postData);
        VLAN ??= 0;
        foreach (var device in _Device.Devices)
        {
            foreach (var MAC in MACList)
            {
                WakeOnLAN.Send(device, MAC, (ushort)VLAN);
            }
        }
        return Ok();
    }
}