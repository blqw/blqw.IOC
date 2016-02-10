using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{
    /// <summary>
    /// 插件容器
    /// </summary>
    public sealed class PlugInContainer : Container
    {
        public void Add(PlugIn plugin)
        {
            plugin.NotNull()?.Throw(nameof(plugin));
            base.Add(plugin, plugin.Name);
        }

        public T Get<T>()
        {
            if (typeof(T).IsSubclassOf(typeof(Delegate)))
            {
                var method = typeof(T).GetMethod("Invoke");
                foreach (PlugIn plugin in this.Components)
                {
                    if (plugin.IsMethod && plugin.CompareMethodSign(method))
                    {
                        return (T)(object)plugin.CreateDelegate(typeof(T));
                    }
                }
            }


            throw new NotImplementedException();
        }

        public void Adds(ComposablePart part)
        {
            part.NotNull()?.Throw(nameof(part));
            foreach (var definition in part.ExportDefinitions)
            {
                var plugin = new PlugIn(part, definition);
                base.Add(plugin, plugin.Name);
            }
        }


        public override void Add(IComponent component)
        {
            component.Is<PlugIn>()?.Throw(nameof(component));
            base.Add(component);
        }

        public override void Add(IComponent component, string name)
        {
            component.Is<PlugIn>()?.Throw(nameof(component));
            ((PlugIn)component).Name = name;
            base.Add(component, name);
        }

        protected override void ValidateName(IComponent component, string name)
        {
            base.ValidateName(component, null);
        }

    }
}
