using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

namespace PTL.ATT.Smoothers
{
    [Serializable]
    public abstract class Smoother
    {
        public static IEnumerable<Smoother> Available
        {
            get { return Assembly.GetAssembly(typeof(Smoother)).GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Smoother))).Select(t => Activator.CreateInstance(t)).Cast<Smoother>(); }
        }

        public abstract void Apply(Prediction prediction);

        public virtual string GetSmoothingDetails()
        {
            return GetType().FullName + ":  ";
        }
    }
}
