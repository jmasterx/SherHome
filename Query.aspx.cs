using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Query : System.Web.UI.Page
{
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        
        Response.ContentType = "application/json; charset=utf-8";

        string json;
        using (var reader = new StreamReader(Request.InputStream))
        {
            json = reader.ReadToEnd();
        }

        List<Querydata> slides = InQuerydata(json);

        List<Kijiji> kijiji = InKijiji();

        List<Aireamenagee> aireamenagee = InAireamenagee();
        List<Aireamenagee> park = new List<Aireamenagee>();
        List<Aireamenagee> golf = new List<Aireamenagee>();

        List<Attraits> attraits = InAttraits();
        attraits = killNulls(attraits);

        List<Bain> bain = InBain();
        bain = killNulls(bain);

        List<Batiment> batiment = InBatiment();
        List<Batiment> hospital = new List<Batiment>();
        List<Batiment> school = new List<Batiment>();
        List<Batiment> store = new List<Batiment>();


        List<Busstops> busstops = InBusstops();

        List<Campsjour> campsjour = InCampsjour();

        List<Ecocentre> ecocentre = InEcocentre();
        ecocentre = killNulls(ecocentre);

        List<Evenements> evenements = InEvenements();

        List<Graffitis> graffitis = InGraffitis();
        graffitis = killNulls(graffitis);

        List<Restaurant> restaurants = InRestaurants();
        restaurants = killNulls(restaurants);

        List <Zap> zaps = InZap();
        zaps = killNulls(zaps);

        List<Queryreturn> queryreturnLIST = new List<Queryreturn>();

        ////////////////////////////////////////////////// SET TITLES ///////////////////////////////////////

        foreach (Aireamenagee a in aireamenagee)
        {
            a.displayTitle = a.NOM;
            a.displayLatitude = "" + a.latitude;
            a.displayLongitude = "" + a.longitude;
            a.markerDisplayType = a.displayTitle;
            if (a.TYPE == "Terrain de golf")
            {
                a.markerDisplayType = "golf";
                golf.Add(a);
            }
                
            else {
                a.markerDisplayType = "parks";
                park.Add(a);
            }
        }

        foreach (Attraits a in attraits)
        {
            a.displayTitle = a.nom;
            a.displayLatitude = a.latitude;
            a.displayLongitude = a.longitude;
            a.markerDisplayType = "attractions";
        }

        foreach (Bain a in bain)
        {
            a.displayTitle = a.DESCRIP;
            a.displayLatitude = a.latitude;
            a.displayLongitude = a.longitude;
            a.markerDisplayType = a.displayTitle;
        }

        List<Batiment> temp = new List<Batiment>();
        foreach (Batiment a in batiment) {
            if (a.SOUSTYPE == "Municipal")
                continue;
            else
            {
                a.displayLatitude = a.latitude;
                a.displayLongitude = a.longitude;
                a.markerDisplayType = a.displayTitle;
                switch (a.SOUSTYPE)
                {


                case "Commerce": a.markerDisplayType = "stores"; a.displayTitle = "Store"; store.Add(a); break;
                                    case "École": a.markerDisplayType = "schools"; a.displayTitle = "School"; school.Add(a); break;
                                    case "Hôpital": a.markerDisplayType = "hospitals"; a.displayTitle = "Hospital or clinic"; hospital.Add(a); break;
                            }
                                temp.Add(a);
                            }
                        }
                        batiment = temp;



            foreach (Busstops a in busstops)
        {
            a.displayTitle = a.stop_name;
            a.displayLatitude = a.stop_lat;
            a.displayLongitude = a.stop_lon;
            a.markerDisplayType = "bus";
        }

        foreach (Campsjour a in campsjour)
        {
            a.displayTitle = a.NOM_CAMP;
            a.displayLatitude = a.latitude;
            a.displayLongitude = a.longitude;
            a.markerDisplayType = "camps";
        }

        foreach (Ecocentre a in ecocentre)
        {
            a.displayTitle = a.ECOCENTRE;
            a.displayLatitude = a.latitude;
            a.displayLongitude = a.longitude;
            a.markerDisplayType = "ecocenters";
        }

        foreach (Evenements a in evenements)
        {
            a.displayTitle = a.TITRE;
            a.displayLatitude = a.latitude;
            a.displayLongitude = a.longitude;
            a.markerDisplayType = "events";
        }

        foreach (Graffitis a in graffitis)
        {
            a.displayTitle = a.LOC;
            a.displayLatitude = a.latitude;
            a.displayLongitude = a.longitude;
            a.markerDisplayType = "graffiti";
        }

        foreach (Restaurant a in restaurants)
        {
            a.displayTitle = a.Nom;
            a.displayLatitude = a.latitude;
            a.displayLongitude = a.longitude;
            a.markerDisplayType = "restaurants";
        }

        foreach (Zap a in zaps)
        {
            a.displayTitle = a.name;
            a.displayLatitude = a.latitude;
            a.displayLongitude = a.longitude;
            a.markerDisplayType = "zap";
        }

        /////////////////////////////////////////////// SEARCH METHODS ///////////////////////////////////
        double golfRadius = 2;
        double parkRadius = 1;
        double attraitsRadius = 1;
        double schoolRadius = 1;
        double hospitalRadius = 1;
        double storeRadius = 1;
        double busRadius = 0.1;
        double campsjourRadius = 2;
        double ecocentreRadius = 1;
        double evenementsRadius = 1;
        double graffitiRadius = 2;
        double restaurantRadius = 0.5;
        double zapRadius = 0.2;

        double sumOfSliders = 0;
        List<double> ranks = new List<double>();
        foreach(Querydata q in slides)
        {
            ranks.Add(q.attractions);
            ranks.Add(q.bus);
            ranks.Add(q.camps);
            ranks.Add(q.ecocenters);
            ranks.Add(q.events);
            ranks.Add(q.golf);
            ranks.Add(q.graffiti);
            ranks.Add(q.hospitals);
            ranks.Add(q.parks);
            ranks.Add(q.restaurants);
            ranks.Add(q.schools);
            ranks.Add(q.stores);
            ranks.Add(q.zap);
        }

        foreach (double d in ranks)
            sumOfSliders += Math.Abs(d);



        foreach (Kijiji k in kijiji)
        {
            if(k.image == "")
            {
                continue;
            }

            foreach (Querydata q in slides)
            {
                Boolean found = false;
                double match = 1;
                Queryreturn thisQueryreturn = new Queryreturn(k, new List<Location>());
                queryreturnLIST.Add(thisQueryreturn);


                foreach (Aireamenagee b in golf)
                {
                    double Blatitude = b.latitude;
                    double Blongitude = b.longitude;

                    double x = convertLat(Blatitude - k.latitude);
                    double y = convertLon(Blatitude - k.latitude, Blongitude - k.longitude);

                    double distance = x * x + y * y;

                    if (distance <= golfRadius)
                    {
                        found = true;
                        thisQueryreturn.results.Add(b);
                        break;
                    }

                }
                if (found && q.golf < 0)
                    match += q.golf / sumOfSliders;
                if (!found && q.golf > 0)
                    match -= q.golf / sumOfSliders;
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                found = false;
                foreach (Aireamenagee b in park)
                {
                    double Blatitude = b.latitude;
                    double Blongitude = b.longitude;

                    double x = convertLat(Blatitude - k.latitude);
                    double y = convertLon(Blatitude - k.latitude, Blongitude - k.longitude);

                    double distance = x * x + y * y;

                    if (distance <= parkRadius)
                    {
                        thisQueryreturn.results.Add(b);
                        found = true;
                        break;
                    }
                }
                if (found && q.parks < 0)
                    match += q.parks / sumOfSliders;
                if (!found && q.parks > 0)
                    match -= q.parks / sumOfSliders;

                /////////////////////////////////////////////////////////////////////////////////////////////////////
                found = false;
                foreach (Attraits a in attraits)
                {
                    double Blatitude = double.Parse(a.latitude);
                    double Blongitude = double.Parse(a.longitude);

                    double x = convertLat(Blatitude - k.latitude);
                    double y = convertLon(Blatitude - k.latitude, Blongitude - k.longitude);

                    double distance = x * x + y * y;

                    if (distance <= attraitsRadius)
                    {
                        thisQueryreturn.results.Add(a);
                        found = true;
                        break;
                    }
                }
                if (found && q.attractions < 0)
                    match += q.attractions / sumOfSliders;
                if (!found && q.attractions > 0)
                    match -= q.attractions / sumOfSliders;
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                found = false;
                foreach (Batiment b in school)
                {
                    double Blatitude = double.Parse(b.latitude);
                    double Blongitude = double.Parse(b.longitude);

                    double x = convertLat(Blatitude - k.latitude);
                    double y = convertLon(Blatitude - k.latitude, Blongitude - k.longitude);

                    double distance = x * x + y * y;

                    if (distance <= schoolRadius)
                    {
                        thisQueryreturn.results.Add(b);
                        found = true;
                        break;
                    }
                }
                if (found && q.schools < 0)
                    match += q.schools / sumOfSliders;
                if (!found && q.schools > 0)
                    match -= q.schools / sumOfSliders;
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                found = false;
                foreach (Batiment b in hospital)
                {
                    double Blatitude = double.Parse(b.latitude);
                    double Blongitude = double.Parse(b.longitude);

                    double x = convertLat(Blatitude - k.latitude);
                    double y = convertLon(Blatitude - k.latitude, Blongitude - k.longitude);

                    double distance = x * x + y * y;

                    if (distance <= hospitalRadius)
                    {
                        thisQueryreturn.results.Add(b);
                        found = true;
                        break;
                    }
                }
                if (found && q.hospitals < 0)
                    match += q.hospitals / sumOfSliders;
                if (!found && q.hospitals > 0)
                    match -= q.hospitals / sumOfSliders;
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                found = false;
                foreach (Batiment b in store)
                {
                    double Blatitude = double.Parse(b.latitude);
                    double Blongitude = double.Parse(b.longitude);

                    double x = convertLat(Blatitude - k.latitude);
                    double y = convertLon(Blatitude - k.latitude, Blongitude - k.longitude);

                    double distance = x * x + y * y;

                    if (distance <= storeRadius)
                    {
                        thisQueryreturn.results.Add(b);
                        found = true;
                        break;
                    }
                }
                if (found && q.stores < 0)
                    match += q.stores / sumOfSliders;
                if (!found && q.stores > 0)
                    match -= q.stores / sumOfSliders;
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                found = false;
                foreach (Busstops b in busstops)
                {
                    double Blatitude = double.Parse(b.stop_lat);
                    double Blongitude = double.Parse(b.stop_lon);

                    double x = convertLat(Blatitude - k.latitude);
                    double y = convertLon(Blatitude - k.latitude, Blongitude - k.longitude);

                    double distance = x * x + y * y;

                    if (distance <= busRadius)
                    {
                        thisQueryreturn.results.Add(b);
                        found = true;
                        break;
                    }
                }
                if (found && q.bus < 0)
                    match += q.bus / sumOfSliders;
                if (!found && q.bus > 0)
                    match -= q.bus / sumOfSliders;
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                found = false;
                foreach (Campsjour b in campsjour)
                {
                    double Blatitude = double.Parse(b.latitude);
                    double Blongitude = double.Parse(b.longitude);

                    double x = convertLat(Blatitude - k.latitude);
                    double y = convertLon(Blatitude - k.latitude, Blongitude - k.longitude);

                    double distance = x * x + y * y;

                    if (distance <= campsjourRadius)
                    {
                        thisQueryreturn.results.Add(b);
                        found = true;
                        break;
                    }
                }
                if (found && q.camps < 0)
                    match += q.camps / sumOfSliders;
                if (!found && q.camps > 0)
                    match -= q.camps / sumOfSliders;
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                found = false;
                foreach (Ecocentre b in ecocentre)
                {
                    double Blatitude = double.Parse(b.latitude);
                    double Blongitude = double.Parse(b.longitude);

                    double x = convertLat(Blatitude - k.latitude);
                    double y = convertLon(Blatitude - k.latitude, Blongitude - k.longitude);

                    double distance = x * x + y * y;

                    if (distance <= ecocentreRadius)
                    {
                        thisQueryreturn.results.Add(b);
                        found = true;
                        break;
                    }
                }
                if (found && q.ecocenters < 0)
                    match += q.ecocenters / sumOfSliders;
                if (!found && q.ecocenters > 0)
                    match -= q.ecocenters / sumOfSliders;
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                found = false;
                foreach (Evenements b in evenements)
                {
                    double Blatitude = double.Parse(b.latitude);
                    double Blongitude = double.Parse(b.longitude);

                    double x = convertLat(Blatitude - k.latitude);
                    double y = convertLon(Blatitude - k.latitude, Blongitude - k.longitude);

                    double distance = x * x + y * y;

                    if (distance <= evenementsRadius)
                    {
                        thisQueryreturn.results.Add(b);
                        found = true;
                        break;
                    }
                }
                if (found && q.events < 0)
                    match += q.events / sumOfSliders;
                if (!found && q.events > 0)
                    match -= q.events / sumOfSliders;
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                found = false;
                foreach (Graffitis b in graffitis)
                {
                    double Blatitude = double.Parse(b.latitude);
                    double Blongitude = double.Parse(b.longitude);

                    double x = convertLat(Blatitude - k.latitude);
                    double y = convertLon(Blatitude - k.latitude, Blongitude - k.longitude);

                    double distance = x * x + y * y;

                    if (distance <= graffitiRadius)
                    {
                        thisQueryreturn.results.Add(b);
                        found = true;
                        break;
                    }
                }
                if (found && q.graffiti < 0)
                    match += q.graffiti / sumOfSliders;
                if (!found && q.graffiti > 0)
                    match -= q.graffiti / sumOfSliders;
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                found = false;
                foreach (Restaurant r in restaurants)
                {
                    double Blatitude = double.Parse(r.latitude);
                    double Blongitude = double.Parse(r.longitude);

                    double x = convertLat(Blatitude - k.latitude);
                    double y = convertLon(Blatitude - k.latitude, Blongitude - k.longitude);

                    double distance = x * x + y * y;

                    if (distance <= restaurantRadius)
                    {
                        thisQueryreturn.results.Add(r);
                        found = true;
                        break;
                    }
                }
                if (found && q.restaurants < 0)
                    match += q.restaurants / sumOfSliders;
                if (!found && q.restaurants > 0)
                    match -= q.restaurants / sumOfSliders;


                /////////////////////////////////////////////////////////////////////////////////////////////////////
                found = false;
                foreach (Zap b in zaps)
                {
                    double Blatitude = double.Parse(b.latitude);
                    double Blongitude = double.Parse(b.longitude);

                    double x = convertLat(Blatitude - k.latitude);
                    double y = convertLon(Blatitude - k.latitude, Blongitude - k.longitude);

                    double distance = x * x + y * y;

                    if (distance <= zapRadius)
                    {
                        thisQueryreturn.results.Add(b);
                        found = true;
                        break;
                    }

                }
                if (found && q.zap < 0)
                    match += q.zap / sumOfSliders;
                if (!found && q.zap > 0)
                    match -= q.zap / sumOfSliders;
                thisQueryreturn.ad.rank = match;
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




        //queryreturnLIST is a list of Queryreturn objects, that have:
        //   public Kijiji ad;
        //   public List<Location> results;

        StringBuilder sb = new StringBuilder("{");
        sb.AppendLine();
        sb.Append(" \"results\":[");
        bool outerFirst = true;

        int i = 0;
        queryreturnLIST = queryreturnLIST.OrderByDescending(ee => ee.ad.rank).ToList();
        if(queryreturnLIST.Count > 100)
        {
            queryreturnLIST = queryreturnLIST.Take(100).ToList();
        }
        foreach (Queryreturn tuple in queryreturnLIST)
        {
            sb.AppendLine();
            if (!outerFirst)
                sb.Append(",");
            else
                outerFirst = false;
            sb.Append("     {");
            sb.AppendLine();
            sb.Append("        \"rank\":\"");
            sb.Append("" + tuple.ad.rank + "\",");
            sb.AppendLine();
            sb.Append("        \"markers\":[");
            sb.AppendLine();
            bool innerFirst = true;
            foreach (Location loc in tuple.results)
            {
                if (!innerFirst)
                    sb.Append(",");
                else
                    innerFirst = false;
                sb.Append("             {");
                sb.AppendLine();
                sb.Append("                 \"latitude\":\"" + EscapeStringValue(loc.displayLatitude) + "\",");
                sb.AppendLine();
                sb.Append("                 \"longitude\":\"" + EscapeStringValue(loc.displayLongitude) + "\",");
                sb.AppendLine();
                sb.Append("                 \"title\":\"" + EscapeStringValue(loc.displayTitle) + "\",");
                sb.AppendLine();
                sb.Append("                 \"type\":\"" + EscapeStringValue(loc.markerDisplayType) + "\"");
                sb.AppendLine();
                sb.Append("             }");
                sb.AppendLine();
            }
            sb.Append("        ],");

            sb.Append("        \"ad\":" + JsonConvert.SerializeObject(tuple.ad));
            sb.AppendLine();
            sb.Append("     }");
        }
        sb.AppendLine();
        sb.Append("]}");
        sb.AppendLine();

        Response.Write(sb);


    }

 public static string EscapeStringValue(string value)
    {
        if(value == null)
        {
            return null;
        }

        const char BACK_SLASH = '\\';
        const char SLASH = '/';
        const char DBL_QUOTE = '"';

        var output = new StringBuilder(value.Length);
        foreach (char c in value)
        {
            switch (c)
            {
                case SLASH:
                    output.AppendFormat("{0}{1}", BACK_SLASH, SLASH);
                    break;

                case BACK_SLASH:
                    output.AppendFormat("{0}{0}", BACK_SLASH);
                    break;

                case DBL_QUOTE:
                    output.AppendFormat("{0}{1}", BACK_SLASH, DBL_QUOTE);
                    break;

                default:
                    output.Append(c);
                    break;
            }
        }

        return output.ToString();
    }

    ////////////////////////////////////////////////// LATITUDE CONVERSION METHODS ///////////////////////////////////////
    public static double convertLat(double latitude)
    {
        return latitude * 110.574;
    }

    public static double convertLon(double latitude, double longitude)
    {
        return longitude * (111.32 * Math.Cos(latitude));
    }

    ////////////////////////////////////////////////// QUERY FIELDS AND METHODS ///////////////////////////////////////

    static double[] queryData;

    static double[] weighedRates;



    //////////////////////////////////////////////////////// KILL NULL METHODS /////////////////////////////////////////////

    public List<Attraits> killNulls(List<Attraits> list)
    {
        List<Attraits> results = new List<Attraits>();
        foreach (Attraits r in list)
            if (r.latitude == "" || r.longitude == "")
                continue;
            else
                results.Add(r);
        return results;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public List<Bain> killNulls(List<Bain> list)
    {
        List<Bain> results = new List<Bain>();
        foreach (Bain r in list)
        {
            if (r.GEOM.Substring(7, 1) == "4")
            {
                r.latitude = r.GEOM.Substring(7, 10);
                r.longitude = r.GEOM.Substring(24, 10);
                results.Add(r);
            }
        }
        return results;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public List<Ecocentre> killNulls(List<Ecocentre> list)
    {
        List<Ecocentre> results = new List<Ecocentre>();
        foreach (Ecocentre r in list)
        {
            r.latitude = r.GEOM.Substring(6, 10);
            r.longitude = r.GEOM.Substring(17, 10);
            results.Add(r);
        }
        return results;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public List<Evenements> killNulls(List<Evenements> list)
    {
        List<Evenements> results = new List<Evenements>();
        foreach (Evenements r in list)
            if (r.latitude.Substring(0, 1) == "4")
                results.Add(r);
        return results;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public List<Graffitis> killNulls(List<Graffitis> list)
    {
        List<Graffitis> results = new List<Graffitis>();
        foreach (Graffitis r in list)
        {
            r.longitude = r.GEOM.Substring(6, 10);
            r.latitude = r.GEOM.Substring(17, 9);
            results.Add(r);
        }
        return results;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public List<Restaurant> killNulls(List<Restaurant> list)
    {
        List<Restaurant> results = new List<Restaurant>();
        foreach (Restaurant r in list)
            if (r.latitude == "" || r.longitude == "")
                continue;
            else
                results.Add(r);
        return results;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public List<Zap> killNulls(List<Zap> list)
    {
        List<Zap> results = new List<Zap>();
        foreach (Zap r in list)
            if (r.latitude.Substring(0, 1) == "4")
                results.Add(r);
        return results;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////





    /////////////////////////////////////////////////////////// DATA CLASSES ////////////////////////////////////////
    public class Location
    {
        public string displayTitle;
        public string displayLatitude;
        public string displayLongitude;
        public string markerDisplayType;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Kijiji : Location
    {
        public readonly string displayType = "Kijiji ad";
        public string title;
        public string image;
        public string price;
        public float latitude;
        public float longitude;
		public string desc;
        public string address;
        public string url;
        public double rank;
    }
    public List<Kijiji> InKijiji()
    {
        var val = Cache["_kij"];
        if (val != null)
            return (List<Kijiji>)val;
        string a = HttpContext.Current.Server.MapPath("~/Tests/kijiji2.json");
        using (StreamReader r = new StreamReader(a))
        {
            string json = r.ReadToEnd();
            Cache["kij"] = JsonConvert.DeserializeObject<List<Kijiji>>(json);
            return (List<Kijiji>)Cache["kij"];
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Aireamenagee : Location
    {
        public readonly string displayType = "Parks and golf";
        public double latitude;
        public double longitude;
        public string NOM;
        public string TYPE;
    }
    public List<Aireamenagee> InAireamenagee()
    {
        var val = Cache["_air"];
        if (val != null)
            return (List<Aireamenagee>)val;
        string a = HttpContext.Current.Server.MapPath("~/Tests/aireamenagee.json");
        using (StreamReader r = new StreamReader(a))
        {
            string json = r.ReadToEnd();
            Cache["air"] = JsonConvert.DeserializeObject<List<Aireamenagee>>(json);
            return (List<Aireamenagee>)Cache["air"];
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Attraits : Location
    {
        public readonly string displayType = "Attractions";
        public string nom;
        public string latitude;
        public string longitude;
    }
    public List<Attraits> InAttraits()
    {
        var val = Cache["_attraits"];
        if (val != null)
            return (List<Attraits>)val;
        string a = HttpContext.Current.Server.MapPath("~/Tests/attraits.json");
        using (StreamReader r = new StreamReader(a))
        {
            string json = r.ReadToEnd();
            Cache["attraits"] = JsonConvert.DeserializeObject<List<Attraits>>(json);
            return (List<Attraits>)Cache["attraits"];
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Bain : Location
    {
        public readonly string displayType = "Swimming pool";
        public string latitude;
        public string longitude;
        public string GEOM;
        public string DESCRIP;
    }
    public List<Bain> InBain()
    {
        string a = HttpContext.Current.Server.MapPath("~/Tests/bain.json");
        using (StreamReader r = new StreamReader(a))
        {
            string json = r.ReadToEnd();
            return JsonConvert.DeserializeObject<List<Bain>>(json);
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Batiment : Location
    {
        public readonly string displayType = "Hospitals, schools and stores";
        public string latitude;
        public string longitude;
        public string SOUSTYPE;
    }
    public List<Batiment> InBatiment()
    {
        var val = Cache["_bat"];
        if (val != null)
            return (List<Batiment>)val;
        string a = HttpContext.Current.Server.MapPath("~/Tests/batiment.json");
        using (StreamReader r = new StreamReader(a))
        {
            string json = r.ReadToEnd();
            Cache["bat"] = JsonConvert.DeserializeObject<List<Batiment>>(json);
            return (List<Batiment>)Cache["bat"];
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Busstops : Location
    {
        public readonly string displayType = "Bus stop";
        public string stop_lat;
        public string stop_lon;
        public string stop_name;
    }
    public List<Busstops> InBusstops()
    {
        var val = Cache["_bus"];
        if (val != null)
            return (List<Busstops>)val;
        string a = HttpContext.Current.Server.MapPath("~/Tests/busstops.json");
        using (StreamReader r = new StreamReader(a))
        {
            string json = r.ReadToEnd();
            Cache["bus"] = JsonConvert.DeserializeObject<List<Busstops>>(json);
            return (List<Busstops>) Cache["bus"];
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Campsjour : Location
    {
        public readonly string displayType = "Day camp";
        public string GEOM;
        public string latitude;
        public string longitude;
        public string NOM_CAMP;
    }
    public List<Campsjour> InCampsjour()
    {

        string a = HttpContext.Current.Server.MapPath("~/Tests/campsjour.json");
        using (StreamReader r = new StreamReader(a))
        {
            string json = r.ReadToEnd();
            return JsonConvert.DeserializeObject<List<Campsjour>>(json);
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Ecocentre : Location
    {
        public readonly string displayType = "Ecocenters";
        public string GEOM;
        public string latitude;
        public string longitude;
        public string ECOCENTRE;
    }
    public List<Ecocentre> InEcocentre()
    {
        string a = HttpContext.Current.Server.MapPath("~/Tests/ecocentres.json");
        using (StreamReader r = new StreamReader(a))
        {
            string json = r.ReadToEnd();
            return JsonConvert.DeserializeObject<List<Ecocentre>>(json);
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Evenements : Location
    {
        public readonly string displayType = "Events";
        public string latitude;
        public string longitude;
        public string TITRE;
    }
    public List<Evenements> InEvenements()
    {
        var val = Cache["_evenements"];
        if (val != null)
            return (List<Evenements>) val;
        string a = HttpContext.Current.Server.MapPath("~/Tests/evenements.json");
        using (StreamReader r = new StreamReader(a))
        {
            string json = r.ReadToEnd();
            var result = JsonConvert.DeserializeObject<List<Evenements>>(json);
            Cache["evenements"] = result;
            return result;
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Graffitis : Location
    {
        public readonly string displayType = "Graffiti";
        public string latitude;
        public string longitude;
        public string GEOM;
        public string LOC;
    }
    public List<Graffitis> InGraffitis()
    {
        var val = Cache["_graffitis"];
        if (val != null)
            return (List<Graffitis>)val;

        string a = HttpContext.Current.Server.MapPath("~/Tests/graffitis.json");
        using (StreamReader r = new StreamReader(a))
        {
            string json = r.ReadToEnd();

            Cache["graffitis"] =  JsonConvert.DeserializeObject<List<Graffitis>>(json);
            return (List<Graffitis>)Cache["graffitis"];
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Restaurant : Location
    {
        public readonly string displayType = "Restaurant";
        public string latitude;
        public string longitude;
        public string Nom;
    }
    public List<Restaurant> InRestaurants()
    {
        var val = Cache["_restaurants"];
        if (val != null)
            return (List<Restaurant>)val;

        string a = HttpContext.Current.Server.MapPath("~/Tests/restaurants.json");
        using (StreamReader r = new StreamReader(a))
        {
            string json = r.ReadToEnd();
            Cache["restaurants"] = JsonConvert.DeserializeObject<List<Restaurant>>(json);
            return (List<Restaurant> )Cache["restaurants"];
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Zap : Location
    {
        public readonly string displayType = "Zap";
        public string latitude;
        public string longitude;
        public string name;
    }
    public List<Zap> InZap()
    {
        var val = Cache["_zap"];
        if (val != null)
            return (List<Zap>)val;
        string a = HttpContext.Current.Server.MapPath("~/Tests/zap.json");
        using (StreamReader r = new StreamReader(a))
        {
            string json = r.ReadToEnd();
            Cache["zap"] = JsonConvert.DeserializeObject<List<Zap>>(json);
            return (List < Zap > )Cache["zap"];

        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////


    /////////////////////////////////////////////////////////// QUERY CLASSES ////////////////////////////////////////
    public class Querydata
    {
        public double bus;
        public double zap;
        public double hospitals;
        public double schools;
        public double ecocenters;
        public double events;
        public double camps;
        public double graffiti;
        public double stores;
        public double attractions;
        public double restaurants;
        public double parks;
        public double golf;
    }
    public List<Querydata> InQuerydata(string json)
    {
        return JsonConvert.DeserializeObject<List<Querydata>>(json);
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////

    public class Queryreturn
    {
        public Kijiji ad;
        public List<Location> results;

        public Queryreturn(Kijiji k, List<Location>l)
        {
            this.ad = k;
            this.results = l;
        }
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////









}

