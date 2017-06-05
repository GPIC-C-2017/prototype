using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class PerformanceMeasurer : MonoBehaviour {
    
    public Text TotalCars;
    public Text CarsPerMinute;
    public Text AverageTimeOnTheRoad;
    public Text TimeElapsed;
    public Text AverageSpeed;

    private int reachedTarget;
    private float timePassed; // total in seconds, * 60 for minutes
    
    // Use this for initialization
    void Start() {
        TotalCars.text = 0.ToString();

        StartCoroutine(UpdateAverageTimeOnTheRoad());
        StartCoroutine(UpdateAverageSpeed());
    }

    // Update is called once per frame
    void Update() {
        timePassed += Time.deltaTime;
        TimeElapsed.text = timePassed.ToString("00.00") + "s";
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
            AverageTimeOnTheRoad.text = time.ToString("00.00") + "s";
        }
    }

    IEnumerator UpdateAverageSpeed() {
        while (true) {
            yield return new WaitForSeconds(2f);
            var speed = GetAverageSpeed();
            AverageSpeed.text = speed.ToString("00.00") + " mph";
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

    private double GetAverageSpeed()
    {
        VehicleAgent[] VAs = FindObjectsOfType<VehicleAgent>();
        double total = 0f;
        foreach (VehicleAgent a in VAs) {
            try {
                total += a.GetCurrentSpeed();
            } catch (System.NullReferenceException e) {
                total += 0f;
            }
        }
        total /= VAs.Length;
        return total;
    }
}