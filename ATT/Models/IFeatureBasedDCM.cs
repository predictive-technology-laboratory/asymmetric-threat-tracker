using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PTL.ATT.Models
{
    public interface IFeatureBasedDCM
    {
        void SelectFeatures(Prediction prediction, bool runPredictionAfterSelect);

        int Run(Prediction prediction, int idOfSpatiotemporallyIdenticalPrediction, bool train, bool runFeatureSelection, bool predict);
    }
}
