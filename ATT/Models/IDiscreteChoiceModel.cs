using LAIR.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTL.ATT.Models
{
    public interface IDiscreteChoiceModel
    {
        Set<string> IncidentTypes { get; }

        string ModelDirectory { get; }

        Area TrainingArea { get; }

        int PointSpacing { get; }
    }
}
