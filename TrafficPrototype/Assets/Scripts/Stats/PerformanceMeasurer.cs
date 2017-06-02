using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class PerformanceMeasurer : MonoBehaviour {

    public Text TotalCars;
    public Text CarsPerMinute;
    public Text AverageTimeOnTheRoad;
    
    private int reachedTarget;
    private float timePassed; // total in seconds, * 60 for minutes
    
    // Use this for initialization
    void Start() {
        TotalCars.text = 0.ToString();

        StartCoroutine(UpdateAverageTimeOnTheRoad());
    }

    // Update is called once per frame
    void Update() {
        timePassed += Time.deltaTime;
    }

    public void ReachedTarget() {
        reachedTarget++;
        TotalCars.text = reachedTarget.ToString();
        CarsPerMinute.text = CalculateCarsPerMinute().ToString(CultureInfo.InvariantCulture);
    }

    private float CalculateCarsPerMinute() {
        return reachedTarget / timePassed * 60f;
    }

    IEnumerator UpdateAverageTimeOnTheRoad() {
        while (true) {
            yield return new WaitForSeconds(2f);
            var time = GetAverageTimeOnTheRoad();
            AverageTimeOnTheRoad.text = time.ToString("00.00") + " seconds";
        }
    }

    private double GetAverageTimeOnTheRoad() {
        NavigationAgent[] NAs = FindObjectsOfType<NavigationAgent>();
        double total = 0f;
        foreach (NavigationAgent a in NAs) {
            total += a.JourneySecondsElapsed().Seconds;
        }
        total /= NAs.Length;
        return total;
    }
}