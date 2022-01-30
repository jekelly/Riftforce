using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace Riftforce
{
    public class ElementalViewModel : ReactiveObject
    {
        public uint Strength { get; }
        public string GuildName { get; }
        public ElementalViewModel(ElementalInPlay model)
        {
            this.Strength = model.Elemental.Strength;
            this.GuildName = model.Elemental.Guild.Name;
        }
    }
}
