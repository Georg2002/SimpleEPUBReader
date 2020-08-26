using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer
{
   public static class PageSetter
    {
        public static void SetPageDefinitions(RenderPage Page, Vector PageSize)
        {
            Page.SinglePageOffset = WritingDirectionModifiers.GetPageOffset(Page, PageSize);           
            Page.PageCount =  WritingDirectionModifiers.GetPageCount(Page, PageSize);    
        }
    }
}
