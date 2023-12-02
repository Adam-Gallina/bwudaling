using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slideshow : MonoBehaviour
{
    [SerializeField] private GameObject[] slides;
    private int currSlide;

    [SerializeField] private bool allowLooping;
    [SerializeField] private GameObject nextBtn;
    [SerializeField] private GameObject backBtn;

    private void Awake()
    {
        slides[0].SetActive(true);
        for (int i = 1; i < slides.Length; i++)
            slides[i].SetActive(false);

        backBtn.SetActive(allowLooping);
        nextBtn.SetActive(true);
    }

    public void Next()
    {
        slides[currSlide].SetActive(false);

        if (++currSlide >= slides.Length)
            currSlide = 0;

        slides[currSlide].SetActive(true);
        backBtn.SetActive(allowLooping || currSlide > 0);
        nextBtn.SetActive(allowLooping || currSlide < slides.Length - 1);
    }

    public void Back()
    {
        slides[currSlide].SetActive(false);

        if (--currSlide < 0)
            currSlide = slides.Length - 1;

        slides[currSlide].SetActive(true);
        backBtn.SetActive(allowLooping || currSlide > 0);
        nextBtn.SetActive(allowLooping || currSlide < slides.Length - 1);
    }
}
