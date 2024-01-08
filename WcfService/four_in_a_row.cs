using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfService
{
    public enum Side { None, First_Player, Second_Player };
    public class four_in_a_row
    {
        public Side[,] GameBoard { get; private set; }
        public four_in_a_row(int rows, int cols)
        {
            // Instantiate an empty board
            GameBoard = new Side[rows, cols];
            for (int row = 0; row < this.GameBoard.GetLength(0); row++)
                for (int col = 0; col < this.GameBoard.GetLength(1); col++)
                    this.GameBoard[row, col] = Side.None;
        }

        public bool Tied()
        {
            for (int col = 0; col < this.GameBoard.GetLength(1); col++)
                if (GameBoard[0, col] == Side.None)
                    return false;
            return true;
        }

        public Side Winner()
        {
            /* we have 4 possibilities to win
             * Vertical,Horizontal,Backward,Forward
             * if you do one of them then you are the winner
             */
            for (int row = 0; row < this.GameBoard.GetLength(0); row++)
            {
                for (int col = 0; col < this.GameBoard.GetLength(1); col++)
                {
                    if (GameBoard[row, col] != Side.None &&
                        (VerticalFourInARow(row, col) || HorizontalFourInARow(row, col) || ForwardFourInARow(row, col) || BackwardFourInARow(row, col)))
                        return GameBoard[row, col];
                }
            }
            return Side.None;
        }


        private bool VerticalFourInARow(int row, int col)
        {
            /* checking the winner by vertical */
            if (GameBoard[row, col] == Side.None)
                return false;
            int count = 1;
            int rowCursor = row - 1;
            while (rowCursor >= 0 && GameBoard[rowCursor, col] == GameBoard[row, col])
            {
                count++;
                rowCursor--;
            }
            rowCursor = row + 1;
            while (rowCursor < GameBoard.GetLength(0) && GameBoard[rowCursor, col] == GameBoard[row, col])
            {
                count++;
                rowCursor++;
            }
            if (count < 4)
                return false;
            return true;
        }

        private bool HorizontalFourInARow(int row, int col)
        {
            /* checking the winner by horizontal */
            if (GameBoard[row, col] == Side.None)
                return false;
            int count = 1;
            int colCursor = col - 1;
            while (colCursor >= 0 && GameBoard[row, colCursor] == GameBoard[row, col])
            {
                count++;
                colCursor--;
            }
            colCursor = col + 1;
            while (colCursor < GameBoard.GetLength(1) && GameBoard[row, colCursor] == GameBoard[row, col])
            {
                count++;
                colCursor++;
            }
            if (count < 4)
                return false;
            return true;
        }

        private bool ForwardFourInARow(int row, int col)
        {
            /* checking the winner by forward */
            if (GameBoard[row, col] == Side.None)
                return false;
            int count = 1;
            int rowCursor = row - 1;
            int colCursor = col + 1;
            while (rowCursor >= 0 && colCursor < GameBoard.GetLength(1) && GameBoard[rowCursor, colCursor] == GameBoard[row, col])
            {
                count++;
                rowCursor--;
                colCursor++;
            }
            rowCursor = row + 1;
            colCursor = col - 1;
            while (rowCursor < GameBoard.GetLength(0) && colCursor >= 0 && GameBoard[rowCursor, colCursor] == GameBoard[row, col])
            {
                count++;
                rowCursor++;
                colCursor--;
            }
            if (count < 4)
                return false;
            return true;
        }

        private bool BackwardFourInARow(int row, int col)
        {
            /* checking the winner by backward */
            if (GameBoard[row, col] == Side.None)
                return false;
            int count = 1;
            int rowCursor = row + 1;
            int colCursor = col + 1;
            while (rowCursor < GameBoard.GetLength(0) && colCursor < GameBoard.GetLength(1) && GameBoard[rowCursor, colCursor] == GameBoard[row, col])
            {
                count++;
                rowCursor++;
                colCursor++;
            }
            rowCursor = row - 1;
            colCursor = col - 1;
            while (rowCursor >= 0 && colCursor >= 0 && GameBoard[rowCursor, colCursor] == GameBoard[row, col])
            {
                count++;
                rowCursor--;
                colCursor--;
            }
            if (count < 4)
                return false;
            return true;
        }

        public bool Insert(Side side, int column)
        {
            /* returning true if the piece is none and we can use it */
            for (int row = GameBoard.GetLength(0) - 1; row >= 0; row--)
            {
                if (GameBoard[row, column] == Side.None)
                {
                    GameBoard[row, column] = side;
                    return true;
                }
            }
            return false;
        }

        public int Num_Of_Pieces_In_Col(int column)
        {
            /* returning number of the pieces in the column*/
            int num = 0;
            for (int row = GameBoard.GetLength(0) - 1; row >= 0; row--)
            {
                if (GameBoard[row, column] != Side.None)
                {
                    num++;
                }
            }
            return num;
        }
    }
}
