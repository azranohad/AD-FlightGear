using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace AD_FlightGear
{
    public class DBflightGear
    {
        float avg(float[] x, int size)
        {
            float sum = 0;
            for (int i = 0; i < size; sum += x[i], i++) ;
            return sum / size;
        }

        // returns the variance of X and Y
        float var(float[] x, int size)
        {
            float av = avg(x, size);
            float sum = 0;
            for (int i = 0; i < size; i++)
            {
                sum += x[i] * x[i];
            }
            return sum / size - av * av;
        }

        // returns the covariance of X and Y
        float cov(float[] x, float[] y, int size)
        {
            float sum = 0;
            for (int i = 0; i < size; i++)
            {
                sum += x[i] * y[i];
            }
            sum /= size;

            return sum - avg(x, size) * avg(y, size);
        }


        // returns the Pearson correlation coefficient of X and Y
        float pearson(float[] x, float[] y, int size)
        {
            if (size == 0)
            {
                return 0;
            }
            float tempCov = cov(x, y, size);
            float sqrtt = (float)(System.Math.Sqrt(var(x, size)) * System.Math.Sqrt(var(y, size)));
            if (tempCov == 0 || sqrtt == 0)
            {
                return 0;
            }
            return tempCov / sqrtt;
            //return cov(x, y, size) / (System.Math.Sqrt(var(x, size)) * System.Math.Sqrt(var(y, size)));
        }

        //number of row in csv file
        private int length;
        public int Length
        {
            get { return length; }
            set { length = value; }
        }
        private string[] _listLine;
        public string[] _ListLine
        {
            get { return _listLine;}
            set {_listLine = value;}
        }

        private string _pathCsv;
        public string _PathCsv
        {
            get {return _pathCsv;}
            set {_pathCsv = value;}
        }

        private string _pathCsvReg;
        public string _PathCsvReg
        {
            get { return _pathCsvReg; }
            set { _pathCsvReg = value; }
        }
        private string _pathXml;
        public string _PathXml
        {
            get { return _pathXml;}
            set {_pathXml = value;}
        }

        public int HdgIndex { get; set; }
        public int AltIndex { get; set; }
        public int SpeedIndex { get; set; }
        public int PitchIndex { get; set; }
        public int RollIndex { get; set; }
        public int YawIndex { get; set; }
        public int Throttle0Index { get; set; }
        public int Throttle1Index { get; set; }
        public int RudderIndex { get; set; }
        public int AlieronIndex { get; set; }
        public int ElevatorIndex { get; set; }

        private List<string> _listFeature;
        public List<string> _ListFeature
        {
            get { return _listFeature; }
        }
        private List<Button> _listFeatureBottuns;
        public List<Button> _ListFeatureBottuns
        {
            get { return _listFeatureBottuns; }
        }

        private List<MapVector> mapDb;
        public List<MapVector> MapDb
        {
            get { return mapDb; }
        }

        //constructor
        public DBflightGear()
        {
            _listFeature = new List<string>();
             mapDb = new List<MapVector>();
        }
        public void createListLines(string path)
        {
            _ListLine = File.ReadAllLines(path);
            length = _listLine.Length;

            for (int i = 0; i < length; i++)
            {
                _ListLine[i] = _ListLine[i] + "\n";
            }
        }
        public void createListDataFeature()
        {
            XmlDocument reader = new XmlDocument();
            reader.Load(_PathXml);
            XmlNodeList NodeList = reader.GetElementsByTagName("node");
            XmlNodeList NameList = reader.GetElementsByTagName("name");
            XmlNodeList TypeList = reader.GetElementsByTagName("type");

            int size = NodeList.Count;

            for (int i = 0; i < size; i ++)
            {
                _listFeature.Add(NodeList[i].InnerText);
                if (NodeList[i].InnerText.Equals(_listFeature[0]) && (i > 0))
                {
                    _listFeature.RemoveAt(_listFeature.Count - 1);
                    break;
                }
                mapDb.Add(new MapVector(NameList[i].InnerText,
                    TypeList[i].InnerText,
                    NodeList[i].InnerText, i));
            }
        }
        public void createListButtons()
        {
            this._listFeatureBottuns = new List<Button>();
            length = this.MapDb.Count;
            for (int i = 0; i < length; i++)
            {
                this._listFeatureBottuns.Add(new Button { ButtonContent = this.MapDb[i].Name, ButtonID = (i).ToString() });

            }
        }
        public void createVectors()
        {

            int row = _ListLine.Length;
            for (int i = 0; i < row; i++)
            {
                string[] words = _ListLine[i].Split(',');
                int col = words.Length;

                for (int j = 0; j < col; j++)
                {
                    mapDb[j]._vectorFloat.Add(float.Parse(words[j]));
                }
            }
        }

        public void findIndexFeatures()
        {

            for (int i = 0; i < _listFeature.Count; i++ )
            {
                string s = mapDb[i].Node;
                switch (s)
                {
                    case "/instrumentation/heading-indicator/indicated-heading-deg":
                        HdgIndex = i;
                        break;
                    case "/instrumentation/altimeter/indicated-altitude-ft":
                        AltIndex = i;
                        break;
                    case "/instrumentation/airspeed-indicator/indicated-speed-kt":
                        SpeedIndex = i;
                        break;
                    case "/orientation/pitch-deg":
                        PitchIndex = i;
                        break;
                    case "/orientation/roll-deg":
                        RollIndex = i;
                        break;
                    case "/orientation/side-slip-deg":
                        YawIndex = i;
                        break;
                    case "rudder":
                        RudderIndex = i;
                        break;
                    case "/controls/engines/engine[1]/throttle":
                        Throttle1Index = i;
                        break;
                    case "/controls/engines/engine[0]/throttle":
                        Throttle0Index = i;
                        break;
                    case "/controls/flight/aileron[0]":
                        AlieronIndex = i;
                        break;
                    case "/controls/flight/elevator":
                        ElevatorIndex = i;
                        break;
                    default:
                        break;
                }

            }
        }

        public void findCorrFeatures()
        {

            List<float> checkedList1;
            List<float> checkedList2;
            float tempPearsonReasult, tempThreshold;

            for (int i = 0; i < mapDb.Count; i++)
            {
                checkedList1 = mapDb[i]._vectorFloat;
                for (int j = i + 1; j < mapDb.Count; j++)
                {
                    checkedList2 = mapDb[j]._vectorFloat;
                    tempPearsonReasult = System.Math.Abs(pearson(checkedList1.ToArray(), checkedList2.ToArray(), checkedList1.Count));
                    if (tempPearsonReasult > mapDb[i].CorrResult)
                    {
                        mapDb[i].CorrIndex = j;
                        mapDb[i].CorrResult = tempPearsonReasult;

                    }
                    tempPearsonReasult = 0;
                }
            }
        }
        public void InitializeDBreg()
        {
            createListLines(_pathCsvReg);
            createListDataFeature();
            createVectors();
            findIndexFeatures();
            findCorrFeatures();
        }

        public void InitializeDBrun()
        {
            createListLines(_pathCsv); 
            createListDataFeature();
            createVectors();
            findIndexFeatures();
        }
    }
}



