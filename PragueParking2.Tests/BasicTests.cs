using Microsoft.VisualStudio.TestTools.UnitTesting;
using PragueParking2.Core;
using PragueParking2.Data;

namespace PragueParking2.Tests;

[TestClass]
public class BasicTests
{
    [TestMethod]
    public void Fee_Is_Zero_Within_FreeMinutes()
    {
        var g = new ParkingGarage(2);
        var v = new Car("ABC123");
        g.Park(v, out _);

        // Manipulate check-in time to now (stay within free minutes)
        var spotVeh = g.FindVehicle("ABC123").vehicle!;
        typeof(Vehicle).GetProperty("CheckInUtc")!
            .SetValue(spotVeh, DateTime.UtcNow);

        Assert.IsTrue(g.Checkout("ABC123", out var fee, out var _));
        Assert.AreEqual(0m, fee);
    }

    [TestMethod]
    public void Move_Fails_When_No_Space()
    {
        var g = new ParkingGarage(1); // 1 spot, capacity 4
        g.Park(new Car("CAR111"), out _); // fills the spot (4 units)

        g.Park(new Motorcycle("MC222"), out var mcSpot); // should fail to park
        // If MC didn't park, force add to simulate moving attempt
        if (mcSpot == -1)
        {
            var mc = new Motorcycle("MC222");
            g.Spots[0].Vehicles.Add(mc); // overflow intentionally
        }

        var ok = g.Move("MC222", 1, out var msg);
        Assert.IsFalse(ok);
    }
}