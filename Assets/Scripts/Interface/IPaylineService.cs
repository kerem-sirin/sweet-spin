using System.Collections.Generic;

namespace SweetSpin
{
    /// <summary>
    /// Payline evaluation service
    /// </summary>
    public interface IPaylineService
    {
        void Initialize(PaylinePattern[] patterns);
        List<PaylineWin> EvaluatePaylines(SymbolType[,] grid, int betPerLine);
        int GetPaylineCount();
    }
}
