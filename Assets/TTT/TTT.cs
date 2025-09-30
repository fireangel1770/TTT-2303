using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.XR;
using static Unity.Collections.AllocatorManager;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.Rendering.DebugUI.Table;

public enum PlayerOption
{
    NONE, //0
    X, // 1
    O // 2
}

public class TTT : MonoBehaviour
{
    public int Rows;
    public int Columns;
    [SerializeField] BoardView board;

    PlayerOption currentPlayer = PlayerOption.X;
    Cell[,] cells;

    // Start is called before the first frame update
    void Start()
    {
        cells = new Cell[Columns, Rows];

        board.InitializeBoard(Columns, Rows);

        for(int i = 0; i < Rows; i++)
        {
            for(int j = 0; j < Columns; j++)
            {
                cells[j, i] = new Cell();
                cells[j, i].current = PlayerOption.NONE;
            }
        }
    }

    public void MakeOptimalMove()
    {
        //If the computer can win(has two in a row, and the third space is open), it should do so.
        ///////////////////If the opponent has two in a row, and the third space is open, block them to prevent victory.
        ///////////////////If the board is empty, it is advantageous to take a corner
        ///////////////////If the opponent controls a corner, but the center is open, take the center
        //If a player controls a corner, but not the center, they should take a cell adjacent to the corner they control
        //at this point, the processes of attempting to win/ blocking will likely play out and result in a tie.
        //as a fail - safe, if none of the above happens, take a random cell


        // sum each row/column based on what's in each cell X = 1, O = -1, blank = 0
        // we have a winner if the sum = 3 (X) or -3 (O)
       
        Debug.Log("Current Player: " + currentPlayer + " ----------");

        if (StartingMove())
            return;
        else if (CheckToWin())
            return;
        else if (CheckToBlock())
            return;
        //else if (((cells[0, 2].current != currentPlayer && cells[0, 2].current != PlayerOption.NONE) && (cells[1, 2].current != currentPlayer && cells[1, 2].current != PlayerOption.NONE)))
        //{
        //    Debug.Log("The Big Problem");
        //    SmartPick(2, 2, 0);
        //}
        else if (CheckOpposite())
            return;
        else
            PickRandom();

    }
    void SmartPick(int col, int row, int sum)
    {
        Debug.Log("Sum in SmartPick: " + sum);
        Debug.Log("Check Row Sent Values:  Columns: " + (col) + ", Rows: " + (row));
        ChooseSpace(col, row);
    }

    bool StartingMove()
    {
  
        int sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (cells[j, i].current != PlayerOption.NONE)
                {
                    sum++;
                }
            }
        }
        if (sum == 0 && cells[0, 0].current == PlayerOption.NONE)
        {
            Debug.Log("Placed Corner 00");
            SmartPick(0, 0, sum);
            return true;
        }
        else if (sum == 0 && cells[0, 2].current == PlayerOption.NONE)
        {
            Debug.Log("Placed Corner 02");
            SmartPick(0, 2, sum);
            return true;
        }
        else if (sum == 0 && cells[2, 0].current == PlayerOption.NONE)
        {
            Debug.Log("Placed Corner 20");
            SmartPick(2, 0, sum);
            return true;
        }
        else if (sum == 0 && cells[2, 2].current == PlayerOption.NONE)
        {
            Debug.Log("Placed Corner 22");
            SmartPick(2, 2, sum);
            return true;
        }

        if (sum == 1)
        {
            if (cells[1, 1].current == PlayerOption.NONE)
            {
                Debug.Log("Placed Center");
                SmartPick(1, 1, sum);
                return true;
            }
        }
        return false;
    }
    bool CheckToBlock()
    {
        int col = 0;
        int row = 0;
        // check Rows
        int sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            if (sum == -2 || sum == 2)
            {
                Debug.Log("Blocked in Row");
                SmartPick(col, row, sum);
                return true;
            }
            sum = 0;
            for (int j = 0; j < Columns; j++)
            {
                var value = 0;

                if (cells[j, i].current == PlayerOption.NONE)
                {
                    row = i;
                    col = j;
                }

                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;

            }
        }
        // check columns
        sum = 0;
        for (int j = 0; j < Columns; j++)
        {
            if (sum == -2 || sum == 2)
            {
                Debug.Log("Blocked in column");
                SmartPick(col, row, sum);
                return true;
            }
            sum = 0;
            for (int i = 0; i < Rows; i++)
            {
                if (cells[j, i].current == PlayerOption.NONE)
                {
                    row = i;
                    col = j;
                }
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;
                sum += value;

            }
        }
        // check diagonals
        // top left to bottom right
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            if (cells[i, i].current == PlayerOption.NONE)
            {
                row = i;
                col = i;
            }
            int value = 0;
            if (cells[i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }
        if (sum == -2 || sum == 2)
        {
            Debug.Log("Blocked in diagonal (top left to bottom right)");
            if (cells[0, 0].current == PlayerOption.NONE)
            {
                SmartPick(0, 0, sum);
                return true;
            }
            else if (cells[1, 1].current == PlayerOption.NONE)
            {
                SmartPick(1, 1, sum);
                return true;
            }
            else if (cells[2, 2].current == PlayerOption.NONE)
            {
                SmartPick(2, 2, sum);
                return true;
            }
        }
        // top right to bottom left
        sum = 0;
        for (int i   = 0; i < Rows; i++)
        {
            if (cells[Columns - 1 - i, i].current == PlayerOption.NONE)
            {
                row = i;
                col = i;
            }
            int value = 0;

            if (cells[Columns - 1 - i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[Columns - 1 - i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }
        if (sum == -2 || sum == 2)
        {
            Debug.Log("Blocked in diagonal (top right to bottom left)");
            if (cells[0, 2].current == PlayerOption.NONE)
            {
                SmartPick(0, 2, sum);
                return true;
            }
            else if (cells[1, 1].current == PlayerOption.NONE)
            {
                SmartPick(1, 1, sum);
                return true;
            }
            else if (cells[2, 0].current == PlayerOption.NONE)
            {
                SmartPick(2, 0, sum);
                return true;
            }
        }
        Debug.Log("Done Checking for Block");
        return false;
    }

    bool CheckOpposite()
    {
        int sum = 0;
        if (currentPlayer == PlayerOption.X || currentPlayer == PlayerOption.O)
        {
            if (cells[0, 0].current == currentPlayer)
            {
                if (cells[2, 0].current == PlayerOption.NONE)
                {
                    Debug.Log("Placed against corner 1");
                    SmartPick(2, 0, sum);
                    return true;
                }
                else if (cells[0, 2].current == PlayerOption.NONE)
                {
                    Debug.Log("Placed against corner 1.5");
                    SmartPick(0, 2, sum);
                    return true;
                }
            }
            else if (cells[0, 2].current == currentPlayer)
            {
                if (cells[0, 0].current == PlayerOption.NONE)
                {
                    Debug.Log("Placed against corner 2");
                    SmartPick(0, 0, sum);
                    return true;
                }
                else if (cells[2, 2].current == PlayerOption.NONE)
                {
                    Debug.Log("Placed against corner 2.5");
                    SmartPick(2, 2, sum);
                    return true;
                }
            }
            else if (cells[2, 0].current == currentPlayer)
            {
                if (cells[0, 0].current == PlayerOption.NONE)
                {
                    Debug.Log("Placed against corner 3");
                    SmartPick(0, 0, sum);
                    return true;
                }
                else if (cells[2, 2].current == PlayerOption.NONE)
                {
                    Debug.Log("Placed against corner 3.5");
                    SmartPick(2, 2, sum);
                    return true;
                }
            }
            else if (cells[2, 2].current == currentPlayer)
            {
                if (cells[0, 2].current == PlayerOption.NONE)
                {
                    Debug.Log("Placed against corner 4");
                    SmartPick(0, 2, sum);
                    return true;
                }
                else if (cells[2, 0].current == PlayerOption.NONE)
                {
                    Debug.Log("Placed against corner 4.5");
                    SmartPick(2, 0, sum);
                    return true;
                }
            }
        }
        return false;
    }
    void PickRandom()
    {
        //Checking for Random
        Debug.Log("Checking for random");
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (cells[j, i].current == PlayerOption.NONE)
                {
                    Debug.Log("Found Random Row: " + i + " Col: " + j);
                    SmartPick(j, i, 0);
                    return;
                }
            }
        }
    }

    bool CheckToWin()
    {
        int col = 0;
        int row = 0;
        // check Rows
        int sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            if (sum == -2 && currentPlayer == PlayerOption.O)
            {
                Debug.Log("Win in Row");
                SmartPick(col, row, sum);
                return true;
            }
            else if (sum == 2 && currentPlayer == PlayerOption.X)
            {
                Debug.Log("Win in Row");
                SmartPick(col, row, sum);
                return true;
            }
            sum = 0;
            for (int j = 0; j < Columns; j++)
            {
                var value = 0;

                if (cells[j, i].current == PlayerOption.NONE)
                {
                    row = i;
                    col = j;
                }

                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;

            }
        }
        // check columns
        sum = 0;
        for (int j = 0; j < Columns; j++)
        {
            if (sum == -2 && currentPlayer == PlayerOption.O)
            {
                Debug.Log("Win in Row");
                SmartPick(col, row, sum);
                return true;
            }
            else if (sum == 2 && currentPlayer == PlayerOption.X)
            {
                Debug.Log("Win in Row");
                SmartPick(col, row, sum);
                return true;
            }
            sum = 0;
            for (int i = 0; i < Rows; i++)
            {
                if (cells[j, i].current == PlayerOption.NONE)
                {
                    row = i;
                    col = j;
                }
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;
                sum += value;

            }
        }
        // check diagonals
        // top left to bottom right
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            if (cells[i, i].current == PlayerOption.NONE)
            {
                row = i;
                col = i;
            }
            int value = 0;
            if (cells[i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }
        if ((sum == -2 && currentPlayer == PlayerOption.O))
        {
            Debug.Log("Win in diagonal (top left to bottom right)");
            if (cells[0, 0].current == PlayerOption.NONE)
            {
                SmartPick(0, 0, sum);
                return true;
            }
            else if (cells[1, 1].current == PlayerOption.NONE)
            {
                SmartPick(1, 1, sum);
                return true;
            }
            else if (cells[2, 2].current == PlayerOption.NONE)
            {
                SmartPick(2, 2, sum);
                return true;
            }
        }
        else if (sum == -2 && currentPlayer == PlayerOption.X)
        {
            Debug.Log("Win in diagonal (top left to bottom right)");
            if (cells[0, 0].current == PlayerOption.NONE)
            {
                SmartPick(0, 0, sum);
                return true;
            }
            else if (cells[1, 1].current == PlayerOption.NONE)
            {
                SmartPick(1, 1, sum);
                return true;
            }
            else if (cells[2, 2].current == PlayerOption.NONE)
            {
                SmartPick(2, 2, sum);
                return true;
            }
        }
        // top right to bottom left
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            if (cells[Columns - 1 - i, i].current == PlayerOption.NONE)
            {
                row = i;
                col = i;
            }
            int value = 0;

            if (cells[Columns - 1 - i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[Columns - 1 - i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }
        if ((sum == -2 && currentPlayer == PlayerOption.O))
        {
            Debug.Log("Win in diagonal (top right to bottom left)");
            if (cells[0, 2].current == PlayerOption.NONE)
            {
                SmartPick(0, 2, sum);
                return true;
            }
            else if (cells[1, 1].current == PlayerOption.NONE)
            {
                SmartPick(1, 1, sum);
                return true;
            }
            else if (cells[2, 0].current == PlayerOption.NONE)
            {
                SmartPick(2, 0, sum);
                return true;
            }
        }
        else if ((sum == 2 && currentPlayer == PlayerOption.X))
        {
            Debug.Log("Win in diagonal (top right to bottom left)");
            if (cells[0, 2].current == PlayerOption.NONE)
            {
                SmartPick(0, 2, sum);
                return true;
            }
            else if (cells[1, 1].current == PlayerOption.NONE)
            {
                SmartPick(1, 1, sum);
                return true;
            }
            else if (cells[2, 0].current == PlayerOption.NONE)
            {
                SmartPick(2, 0, sum);
                return true;
            }
        }

        Debug.Log("Done Checking for Win");
        return false;
    }
    public void ChooseSpace(int column, int row)
    {
        // can't choose space if game is over
        if (GetWinner() != PlayerOption.NONE)
            return;

        // can't choose a space that's already taken
        if (cells[column, row].current != PlayerOption.NONE)
            return;

        // set the cell to the player's mark
        cells[column, row].current = currentPlayer;

        // update the visual to display X or O
        board.UpdateCellVisual(column, row, currentPlayer);

        // if there's no winner, keep playing, otherwise end the game
        if(GetWinner() == PlayerOption.NONE)
            EndTurn();
        else
        {
            Debug.Log("GAME OVER!");
        }
        return;
    }

    public void EndTurn()
    {
        // increment player, if it goes over player 2, loop back to player 1
        currentPlayer += 1;
        if ((int)currentPlayer > 2)
            currentPlayer = PlayerOption.X;
    }

    public PlayerOption GetWinner()
    {
        // sum each row/column based on what's in each cell X = 1, O = -1, blank = 0
        // we have a winner if the sum = 3 (X) or -3 (O)
        int sum = 0;

        // check rows
        for (int i = 0; i < Rows; i++)
        {
            sum = 0;
            for (int j = 0; j < Columns; j++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check columns
        for (int j = 0; j < Columns; j++)
        {
            sum = 0;
            for (int i = 0; i < Rows; i++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check diagonals
        // top left to bottom right
        sum = 0;
        for(int i = 0; i < Rows; i++)
        {
            int value = 0;
            if (cells[i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        // top right to bottom left
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            int value = 0;

            if (cells[Columns - 1 - i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[Columns - 1 - i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        return PlayerOption.NONE;
    }
}
