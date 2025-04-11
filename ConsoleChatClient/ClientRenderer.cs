using System;
using System.Collections.Generic;

namespace ConsoleChatClient;

internal class ClientRenderer
{
    private readonly Queue<Message> messages = new();
    private int scrollOffset = 0; // 用于记录滚动偏移量

    public void AddMessage(Message message)
    {
        bool isAtBottom = scrollOffset == 0; // 检查用户是否在底部
        messages.Enqueue(message);

        if (messages.Count > Console.WindowHeight - 1) // 保证消息队列不会超过可显示的行数
        {
            messages.Dequeue();
        }

        if (isAtBottom) // 如果用户在底部，自动滚动到底部
        {
            scrollOffset = 0;
        }
        else
        {
            // 保持当前滚动位置
            scrollOffset = Math.Min(scrollOffset + 1, messages.Count - (Console.WindowHeight - 1));
        }

        Render();
    }

    public void ScrollUp()
    {
        if (scrollOffset < messages.Count - (Console.WindowHeight - 1))
        {
            scrollOffset++;
            Render();
        }
    }

    public void ScrollDown()
    {
        if (scrollOffset > 0)
        {
            scrollOffset--;
            Render();
        }
    }

    public void Render()
    {
        Console.Clear();
        int displayHeight = Console.WindowHeight - 1; // 除去输入行的高度
        int windowTop = Console.WindowTop; // 当前窗口的顶部行号
        int windowBottom = windowTop + Console.WindowHeight - 1; // 当前窗口的底部行号

        var messagesToDisplay = messages.ToArray();

        for (int i = 0; i < displayHeight; i++)
        {
            int messageIndex = messagesToDisplay.Length - displayHeight - scrollOffset + i;
            if (messageIndex >= 0 && messageIndex < messagesToDisplay.Length)
            {
                Console.SetCursorPosition(0, windowTop + i); // 根据窗口顶部动态调整行号
                Console.WriteLine($"<{messagesToDisplay[messageIndex].sender}>: {messagesToDisplay[messageIndex].message}");
            }
            else
            {
                Console.SetCursorPosition(0, windowTop + i);
                Console.WriteLine(); // 空行
            }
        }

        // 动态调整输入行的位置
        int inputLine = windowBottom; // 输入行始终位于当前窗口的底部
        Console.SetCursorPosition(0, inputLine);
        Console.Write("输入: ");
    }

}
