using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace robotInterface
{
    public class TrajectoryManager
    {

        public readonly Dictionary<string, (float X, float Y)> pointsList = new Dictionary<string, (float X, float Y)>
        {
            {"Centre", (150, 100)},
            {"Zone Haut Gauche", (22, 178)},
            {"Zone Milieu Gauche", (22, 100)},
            {"Zone Bas Gauche", (22, 22)},
            {"Zone Haut Droit", (278, 178)},
            {"Zone Milieu Droit", (278, 100)},
            {"Zone Bas Droit", (278, 22)},
            {"Balise Haut Gauche", (75, 150)},
            {"Balise Bas Gauche", (75, 50)},
            {"Balise Haut Droit", (225, 150)},
            {"Balise Bas Droit", (225, 50)}
        };



    }
}
