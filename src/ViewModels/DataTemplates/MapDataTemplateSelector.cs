using System.Windows;
using System.Windows.Controls;
using SierraHOTAS.Models;

namespace SierraHOTAS.ViewModels.DataTemplates
{
    public class MapDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ButtonDataTemplate { get; set; }
        public DataTemplate LinearDataTemplate { get; set; }
        public DataTemplate RadialDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is IBaseMapViewModel iMap)) return ButtonDataTemplate;

            switch (iMap.Type)
            {
                case HOTASButton.ButtonType.AxisLinear:
                    return LinearDataTemplate;
                case HOTASButton.ButtonType.AxisRadial:
                    return RadialDataTemplate;
                default:
                    return ButtonDataTemplate;
            }
        }
    }
}
