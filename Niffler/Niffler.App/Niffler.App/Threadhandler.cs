
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
//the communication layer between cAlgo and the frame application

namespace Niffler.App
{

   
public class ThreadHandler
{

    private string sMesText;
    private int iDigs;
    //declaring the frame application with event handling
    private Niffler.App.Main withEventsField_myForm;
    private Niffler.App.Main myForm
    {
        get { return withEventsField_myForm; }
        set
        {
            if (withEventsField_myForm != null)
            {
                withEventsField_myForm.ButtonBuyClicked -= myForm_ButtonBuyClicked;
                withEventsField_myForm.ButtonSellClicked -= myForm_ButtonSellClicked;
            }
            withEventsField_myForm = value;
            if (withEventsField_myForm != null)
            {
                withEventsField_myForm.ButtonBuyClicked += myForm_ButtonBuyClicked;
                withEventsField_myForm.ButtonSellClicked += myForm_ButtonSellClicked;
            }
        }
    }

    public event ButtonBuyClickedEventHandler ButtonBuyClicked;
    public delegate void ButtonBuyClickedEventHandler();
    public event ButtonSellClickedEventHandler ButtonSellClicked;
    public delegate void ButtonSellClickedEventHandler();

    //constructor for frame application with overloading parameters
    public ThreadHandler(string sMsgTxt, int iDigits)
    {
        sMesText = sMsgTxt;
        iDigs = iDigits;
    }

    //start the frame application
    public void Work()
    {
        //use the windows visual style
        Application.EnableVisualStyles();
        Application.DoEvents();

        myForm = new Niffler.App.Main ();
        myForm.Text = sMesText;
        myForm.ShowDialog();
    }

    public void setButtonText(string cTxt, double dPrice)
    {
        //passing the price from cAlgo to the form application
        myForm.SetButtonText(cTxt, dPrice);
    }

    private void myForm_ButtonBuyClicked()
    {
        //passing the button click event from frame application to cAlgo
        if (ButtonBuyClicked != null)
        {
            ButtonBuyClicked();
        }
    }

    private void myForm_ButtonSellClicked()
    {
        //passing the button click event from frame application to cAlgo
        if (ButtonSellClicked != null)
        {
            ButtonSellClicked();
        }
    }
}
}