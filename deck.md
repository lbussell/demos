---
title: Slides
subtitle: A tiny Pandoc slide deck
aspectratio: 169
---

# Property Testing
l

# Background
- LLMs generate code really quickly
- The bottleneck is how quickly changes can be validated

# Question
How do we get *absolute* confidence in what the LLM generated?

# Answer
Test it!

# C# code

```csharp
public sealed class Greeter
{
    public string Greet(string name)
    {
        return $"Hello, {name}!";
    }
}
```
