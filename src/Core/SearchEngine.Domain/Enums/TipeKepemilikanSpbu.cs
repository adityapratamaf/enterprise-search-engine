namespace SearchEngine.Domain.Enums;

/// <summary>
/// Pola kepemilikan dan pengelolaan SPBU. Nilainya tercermin pada digit
/// kedua kode SPBU, namun tetap disimpan sebagai kolom tersendiri agar
/// tidak perlu di-parse dari string saat query.
/// </summary>
public enum TipeKepemilikanSpbu
{
    /// <summary>Company Owned, Company Operated.</summary>
    Coco = 1,

    /// <summary>Company Owned, Dealer Operated.</summary>
    Codo = 2,

    /// <summary>Dealer Owned, Dealer Operated.</summary>
    Dodo = 3
}
