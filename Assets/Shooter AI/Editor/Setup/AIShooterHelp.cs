using UnityEngine;
using System.Collections;
using UnityEditor;


public class AIShooterHelp : EditorWindow {


public string helpVideosURL = "http://nikita-makarov.wix.com/gateway-games#!help/cj2w"; //this is the url for all the videos
public string supportURL = "http://nikita-makarov.wix.com/gateway-games#!support/c21nl"; //this is the url to the customer support
public string helpFileSupport = "/Shooter AI/Help/Manual.pdf"; //this is the pdf help file
public string tutorialPlaylist = "http://www.youtube.com/playlist?list=PLrzO1o8MansJl9Qqw-dvh6uCtqB7rCaVd"; //this is the url to the youtube playlist for tutorial vids
public string gatewayImageURL = "Images/gateway_games"; //the url to the gateway image location
public string websiteURL = "http://nikita-makarov.wix.com/gateway-games"; //our website


[MenuItem("Tools/Shooter AI/Help And Support")]
private static void showEditor()
{
EditorWindow.GetWindow (typeof (AIShooterHelp), true, "Shooter AI Help");
}

void OnGUI()
{
//this is the button to help videos
if(GUILayout.Button("Help Videos"))
{
Application.OpenURL(helpVideosURL);
}

if(GUILayout.Button("Support Page"))
{
Application.OpenURL(supportURL);
}

if(GUILayout.Button("Manual"))
{
Application.OpenURL((Application.dataPath) + helpFileSupport);
}

if(GUILayout.Button("Tutorial Playlist"))
{
Application.OpenURL(tutorialPlaylist);
}

if(GUILayout.Button("Our Website"))
{
Application.OpenURL(websiteURL);
}

EditorGUILayout.Space();
EditorGUILayout.Space();
EditorGUILayout.Space();

GUI.Label(new Rect(10, 130, 600, 100), "Special thanks to Trevor Blize for the Capture the Flag demo!", EditorStyles.boldLabel);
GUI.Label(new Rect(10, 155, 600, 100), "Thank you for using Shooter AI \n - Gateway Games Team", EditorStyles.boldLabel);
GUI.Box(new Rect(10, 200, 200 , 100), Resources.Load(gatewayImageURL) as Texture2D);




}



}
