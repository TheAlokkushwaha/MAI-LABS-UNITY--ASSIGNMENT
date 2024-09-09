using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SnakeControl : MonoBehaviour
{
    private enum State
    {
        Alive,
        Dead
    }

    private State state;
    private Vector2Int gridMoveDirection;
    private Vector2Int gridPosition;
    private float gridMoveTimer;
    private float gridMoveTimerMax;
    private List<Transform> segments;
    public Transform segmentPrefab;
    public int initialSize = 4;

    private int score;
    public TextMeshProUGUI scoreText;

    public Button reverseButton;
    private bool isReversed = false;
    private Color reversedColor = Color.red;

    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText;
    public Button restartButton;

    private void Awake()
    {
        gridPosition = new Vector2Int(0, 0);
        gridMoveTimerMax = 0.25f;
        gridMoveTimer = gridMoveTimerMax;
        gridMoveDirection = new Vector2Int(1, 0);

        segments = new List<Transform>();
        segments.Add(this.transform);

        for (int i = 1; i < initialSize; i++)
        {
            segments.Add(Instantiate(segmentPrefab));
        }

        state = State.Alive;
        score = 0;
        UpdateScoreText();

        reverseButton.onClick.AddListener(ToggleReverse);
        restartButton.onClick.AddListener(ResetGame);
        gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        switch (state)
        {
            case State.Alive:
                HandleInput();
                HandleGridMovement();
                break;

            case State.Dead:
                break;
        }
    }

    private void HandleInput()
    {
        if (!isReversed)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && gridMoveDirection.y != -1)
            {
                gridMoveDirection = new Vector2Int(0, 1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && gridMoveDirection.y != 1)
            {
                gridMoveDirection = new Vector2Int(0, -1);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && gridMoveDirection.x != 1)
            {
                gridMoveDirection = new Vector2Int(-1, 0);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && gridMoveDirection.x != -1)
            {
                gridMoveDirection = new Vector2Int(1, 0);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && gridMoveDirection.y != 1)
            {
                gridMoveDirection = new Vector2Int(0, -1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && gridMoveDirection.y != -1)
            {
                gridMoveDirection = new Vector2Int(0, 1);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && gridMoveDirection.x != -1)
            {
                gridMoveDirection = new Vector2Int(1, 0);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && gridMoveDirection.x != 1)
            {
                gridMoveDirection = new Vector2Int(-1, 0);
            }
        }
    }

    private void HandleGridMovement()
    {
        gridMoveTimer += Time.deltaTime;
        if (gridMoveTimer >= gridMoveTimerMax)
        {
            gridMoveTimer -= gridMoveTimerMax;

            for (int i = segments.Count - 1; i > 0; i--)
            {
                segments[i].position = segments[i - 1].position;
            }

            gridPosition += gridMoveDirection;
            segments[0].position = new Vector3(gridPosition.x, gridPosition.y, 0);

            transform.eulerAngles = new Vector3(0, 0, GetAngleFromVector(gridMoveDirection) - 90);

            if (CheckSelfCollision())
            {
                SnakeDied();
            }
        }
    }

    private float GetAngleFromVector(Vector2Int dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        return angle;
    }

    public void Grow()
    {
        Transform segment = Instantiate(segmentPrefab);
        segment.position = segments[segments.Count - 1].position;

        Color currentColor = segments[0].GetComponent<SpriteRenderer>().color;
        segment.GetComponent<SpriteRenderer>().color = currentColor;

        segments.Add(segment);
    }

    public void ResetState()
    {
        for (int i = 1; i < segments.Count; i++)
        {
            Destroy(segments[i].gameObject);
        }

        segments.Clear();
        segments.Add(this.transform);

        for (int i = 1; i < initialSize; i++)
        {
            Transform newSegment = Instantiate(segmentPrefab);
            newSegment.position = segments[i - 1].position - new Vector3(1, 0, 0);
            segments.Add(newSegment);
        }

        gridPosition = new Vector2Int(0, 0);
        gridMoveDirection = new Vector2Int(1, 0);
        segments[0].position = new Vector3(gridPosition.x, gridPosition.y, 0);

        score = 0;
        UpdateScoreText();
        state = State.Alive;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (state == State.Alive)
        {
            if (other.tag == "Food")
            {
                Grow();
                score++;
                UpdateScoreText();
            }
            else if (other.tag == "Obstacle")
            {
                SnakeDied();
            }
        }
    }

    private void SnakeDied()
    {
        state = State.Dead;
        gameOverPanel.SetActive(true);
        gameOverScoreText.text = "Your Score: " + score;
    }

    private bool CheckSelfCollision()
    {
        for (int i = 1; i < segments.Count; i++)
        {
            if (segments[i].position == segments[0].position)
            {
                return true;
            }
        }
        return false;
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }

    private void ToggleReverse()
    {
        isReversed = !isReversed;
        SetSnakeColor(isReversed ? reversedColor : Color.white);
    }

    private void SetSnakeColor(Color color)
    {
        foreach (Transform segment in segments)
        {
            segment.GetComponent<SpriteRenderer>().color = color;
        }
    }

    private void ResetGame()
    {
        ResetState();
        gameOverPanel.SetActive(false);
    }
}
