using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{


    float columnSize = 20;
    float rowSize = 20;

    float margin = 200;

    float originX = 0;
    float originY = 0;

    Color[] colors = {
        Color.red,
        new Color(1.0f, 0.5490196f, 0),
        Color.yellow,
        new Color(0.6f, 0.1960784f, 0.9411765f),
        Color.green,
        Color.cyan,
        Color.blue,
        new Color(0.5450981f, 0.2705882f, 0.07450981f),
        Color.magenta,
        Color.white, Color.gray, Color.black
         };

    public Text ScoreText;

    Bubble[,] BubblesArray = new Bubble[BubbleColumns, BubbleRows];
    const int BubbleColumns = 8;
    const int BubbleRows = 13;
    public GameObject BubbleParameter;

    public GameObject canvas;


    List<Bubble> SelectedBubbles = new List<Bubble>();
    private Material selectedBubbleColor;

    int score = 0;

    private bool AreBubblesSelected = false;
    private bool IsGameOver = false;
    private const int minBubblesToRemove = 2;

    private int colorQuant = 6;


    // Use this for initialization
    void Start()
    {
        ScoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        Camera camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        float aspect = Mathf.Round(camera.aspect * 100f) / 100f;

        //this is to be altered different Windows Phone 8.0 aspect ratios
        //there should be a better way of doing this
        if (aspect == 0.6f) //WXGA or WVGA
            camera.orthographicSize = 5;
        else if (aspect == 0.56f) //720p
        {
            camera.orthographicSize = 5.37f;
            camera.transform.position = new Vector3(camera.transform.position.x, 4.62f, camera.transform.position.z);
        }

        //this is to be used as the color that the selected Bubbles will have
        selectedBubbleColor = Resources.Load("Materials/whiteMaterial") as Material;
        AreBubblesSelected = false;
        IsGameOver = false;

        InitializeBubbles();
        ReallocateBubbles();
        RenderArray();
    }

    /// <summary>
    /// initializes the bubbles
    /// </summary>
    private void InitializeBubbles()
    {

        float area = Screen.width - margin * 2;
        columnSize = area / BubbleColumns;
        rowSize = Screen.height / BubbleRows;

        originX = margin + columnSize / 2;
        originY = rowSize / 2;

        for (int column = 0; column < BubbleColumns; column++)
        {
            for (int row = 0; row < BubbleRows; row++)
            {
                Color color = RandomColor();

                //create a new bubble
                var go = (GameObject)Instantiate(BubbleParameter, new Vector3((float)(column * columnSize + originX), (float)row * rowSize + originY, 0f), Quaternion.identity);
                go.transform.localScale = new Vector3(0.01f * columnSize, 0.01f * rowSize, 0.1f);
                Bubble bubble = go.GetComponent<Bubble>();
                bubble.color = color;
                bubble.column = column;
                bubble.row = row;

                BubblesArray[column, row] = bubble;
                go.name = column.ToString() + "-" + row.ToString();
                go.transform.SetParent(canvas.transform);

            }

        }
    }

    Color RandomColor()
    {
        return colors[UnityEngine.Random.Range(0, colorQuant)];
    }
    void RenderArray()
    {
        for (int column = 0; column < BubbleColumns; column++)
            for (int row = BubbleRows - 1; row >= 0; row--)
            {
                if (BubblesArray[column, row] != null)
                    BubblesArray[column, row].gameObject.transform.position = new Vector3((float)(column * columnSize + originX), (float)row * rowSize + originY, 0f);
            }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.LoadLevel("StartScene");

        if (Input.GetMouseButtonDown(0))
        {
            int col = (int)((Input.mousePosition.x - margin) / columnSize);
            int row = (int)(Input.mousePosition.y / rowSize);
            if (col >= 0 && col < BubbleColumns && row >= 0 && row < BubbleRows)
            {
                BubbleSeleceted(col, row);
                IsGameOver = CheckIsGameOver();
                if (IsGameOver)
                    StartCoroutine(GotoGameOver());
            }
        }
    }

    public void BubbleSeleceted(int column, int row)
    {
        if (BubblesArray[column, row] != null)
        {
            Bubble selectedBubble = BubblesArray[column, row];


            Color color = selectedBubble.color;
            SelectedBubbles.Clear();
            MarkBubbles(column, row, color);

            if (SelectedBubbles.Count > 1)
            {
                score += SelectedBubbles.Count;

                ScoreText.text = "Score " + score;
                foreach (Bubble el in SelectedBubbles)
                {
                    BubblesArray[el.column, el.row] = null;
                    Destroy(el.gameObject);
                }
                ReallocateBubbles();
                RenderArray();

            }
        }
    }

    private void MarkBubbles(int column, int row, Color colorToCompare)
    {
        Bubble bubble;
        bubble = BubblesArray[column, row];

        if (bubble == null)
        {
            return;
        }


        if (bubble.color == colorToCompare)
        {
            if (SelectedBubbles.Contains(bubble)) return; //we're not checking the same Bubble twice, this will incur a stack overflow

            // bubble.color = selectedBubbleColor;
            SelectedBubbles.Add(bubble);

            //check bottom
            if (row > 0)
                MarkBubbles(column, row - 1, colorToCompare);
            if (column > 0) //check left
                MarkBubbles(column - 1, row, colorToCompare);
            if (column < BubbleColumns - 1) //check right
                MarkBubbles(column + 1, row, colorToCompare);
            if (row < BubbleRows - 1) //check top
                MarkBubbles(column, row + 1, colorToCompare);
        }

    }


    private IEnumerator GotoGameOver()
    {
        ScoreManager sm = new ScoreManager();
        sm.AddScore(new ScoreEntry() { ScoreInt = this.score, Date = DateTime.Now });
        yield return new WaitForSeconds(2f);
        Globals.GameScore = score;
        Application.LoadLevel("highScoresScene");
    }


    private bool CheckIsGameOver()
    {
        //if there are any Bubbles selected, there's no point in checking as it's definitely not game over

        for (int column = 0; column <= BubbleColumns - 1; column++)
        {
            for (int row = BubbleRows - 1; row > 0; row--)
            {
                //we are comparing each Bubble with the ones located below and right from it
                if (BubblesArray[column, row] == null) continue;


                if (BubblesArray[column, row].color == BubblesArray[column, row - 1].color)
                    return false;

                if (column < BubbleColumns - 1)
                {
                    if (BubblesArray[column + 1, row] == null) continue;

                    if (BubblesArray[column, row].color == BubblesArray[column + 1, row].color)
                        return false;
                }
            }
        }

        return true;

    }



    private void ReallocateBubbles()
    {
        //first, let's clear the empty spaces in the rows
        for (int column = 0; column < BubbleColumns; column++)
        {

            int lastSpotReplaced = 0;
            for (int row = 0; row < BubbleRows; row++)
            {
                // Remove empty spaces:
                // as empty spaces do not increment lastSpotReplaced
                // every time a substitution occurs the piece moved will be transfered
                // to lastSpotReplaced jumping over empty spaces to the first spot available
                if (BubblesArray[column, row] != null)
                {
                    Bubble b = BubblesArray[column, row];
                    BubblesArray[column, row] = null;

                    b.row = lastSpotReplaced;
                    BubblesArray[column, lastSpotReplaced] = b;
                    lastSpotReplaced++;
                }

            }
        }

        //first, let's clear the empty spaces in the rows
        int lastColumnReplaced = 0;

        for (int column = 0; column < BubbleColumns; column++)
        {

            if (BubblesArray[column, 0] == null)
            {
                lastColumnReplaced++;
            }
            else
            {
                for (int row = 0; row < BubbleRows; row++)
                {
                    Bubble b = BubblesArray[column, row];
                    BubblesArray[column, row] = null;

                    if (b != null)
                    {

                        b.column = column - lastColumnReplaced;
                        b.row = row;
                        BubblesArray[column - lastColumnReplaced, row] = b;
                    }
                }
            }
        }
    }



}
