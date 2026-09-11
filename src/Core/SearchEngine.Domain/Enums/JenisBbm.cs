namespace SearchEngine.Domain.Enums;

/// <summary>
/// Golongan bahan bakar yang dijual SPBU. Menentukan atribut teknis mana
/// yang relevan: <see cref="Gasoline"/> memakai RON,
/// <see cref="Diesel"/> memakai cetane number.
/// </summary>
public enum JenisBbm
{
    Gasoline = 1,

    Diesel = 2
}
