namespace AutoRepairShop.Core.Interfaces;

/// <summary>Об'єкт, для якого можна обчислити вартість у гривнях.</summary>
public interface ICostCalculable
{
    decimal CalculateCost();
}
