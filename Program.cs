// ============================================================================
// 
// アプリケーション
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using MergeNicoKaraTracks.Forms;

using System;
using System.Windows.Forms;

namespace MergeNicoKaraTracks
{
	static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMergeNicoKaraTracks());
        }
    }
	// static class Program ___END___
}
// namespace MergeNicoKaraTracks ___END___
