using SimpleJSON;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GetWeather : MonoBehaviour
{
    [SerializeField]
    private RawImage figMotit;
    [SerializeField]
    private Text cityCountry;
    [SerializeField]
    private Text temperature;

    private string currentIP;
    private string currentCountry;
    private string currentCity;

    private string retrievedCountry;
    private string retrievedCity;
    private int conditionID;
    private string conditionName;
    private string conditionDescription;
    private string conditionImage;

    public void Request()
    {
        StartCoroutine(OnResponse());
        figMotit.gameObject.SetActive(true);
    }

    public IEnumerator OnResponse()
    {
        WWW cityRequest = new WWW("http://www.geoplugin.net/json.gp");
        yield return cityRequest;

        if (cityRequest.error == null || cityRequest.error == "")
        {
            var N = JSON.Parse(cityRequest.text);
            currentCity = N["geoplugin_city"].Value;
            currentCountry = N["geoplugin_countryName"].Value;
            currentIP = N["geoplugin_request"].Value;
        }
        else
        {
            Debug.Log("WWW error: " + cityRequest.error);
        }

        WWW request = new WWW("http://api.openweathermap.org/data/2.5/forecast/daily?q=Saarbrucken" + "&units=metric&cnt=2&APPID=542ffd081e67f4512b705f89d2a611b2");
        yield return request;

        if (request.error == null || request.error == "")
        {
            var N = JSON.Parse(request.text);

            retrievedCity = N["city"]["name"].Value;        //get the country
            retrievedCountry = N["city"]["country"].Value;  //get the city

            string tempMin = N["list"][0]["temp"]["min"].Value; //get the temperature
            float floatMin;                              //variable to hold the parsed temperature
            float.TryParse(tempMin, out floatMin);           //parse the temperature
            int tempMinimal = (int)Math.Round(floatMin);

            string tempMax = N["list"][0]["temp"]["max"].Value; //get the temperature
            float floatMax;                                  //variable to hold the parsed temperature
            float.TryParse(tempMax, out floatMax);           //parse the temperature
            int tempMaximal = (int)Math.Round(floatMax);

            string tempCur = N["list"][0]["temp"]["day"].Value; //get the temperature
            float floatCurrent;                                  //variable to hold the parsed temperature
            float.TryParse(tempCur, out floatCurrent);           //parse the temperature
            int tempCurrent = (int)Math.Round(floatCurrent);
            
            int.TryParse(N["list"][0]["weather"][0]["id"], out conditionID); //get the current condition ID
            conditionName = N["list"][0]["weather"][0]["main"].Value;       //get the current condition Name
            conditionDescription = N["list"][0]["weather"][0]["description"].Value; //get the current condition Description
            conditionImage = N["list"][0]["weather"][0]["icon"].Value;              //get the current condition Image

            cityCountry.text = "" + retrievedCity + ", " + retrievedCountry;
            temperature.text = tempCurrent.ToString() + "°";
        }
        else
        {
            Debug.Log("WWW erroriiiiiii: " + request.error);
        }

        WWW conditionRequest = new WWW("http://openweathermap.org/img/w/" + conditionImage + ".png");
        yield return conditionRequest;

        if (conditionRequest.error == null || conditionRequest.error == "")
        {
            var texture = conditionRequest.texture;
            figMotit.texture = texture;
        }
        else
        {
            Debug.Log("WWW error foto: " + conditionRequest.error);
        }
    }
}
