using System;
using System.Globalization;
using System.Threading;

namespace RomanticWeb.Tests.Helpers
{
    /// <summary>
    /// Source code from the Code Associate C# code library, Full documentation and latest updates can be found
    /// @ http://www.codeassociate.com/caapi/
    /// </summary>
    public class CultureScope : IDisposable
    {
        private CultureInfo _rollbackCulture;

        public CultureScope(CultureInfo cultureinfo)
        {
            this.CommonConstructor(cultureinfo);
        }

        public CultureScope(string culture)
        {
            var cultureinfo = new CultureInfo(culture);
            CommonConstructor(cultureinfo);
        }

        public void Dispose()
        {
#if NETSTANDARD1_6
            CultureInfo.CurrentCulture = this._rollbackCulture;
#else
            System.Threading.Thread.CurrentThread.CurrentCulture = this._rollbackCulture;
#endif
        }

        private void CommonConstructor(CultureInfo cultureinfo)
        {
            _rollbackCulture = CultureInfo.CurrentCulture;
#if NETSTANDARD1_6
            CultureInfo.CurrentCulture = cultureinfo;
#else
            System.Threading.Thread.CurrentThread.CurrentCulture = cultureinfo;
#endif
        }
    }
}