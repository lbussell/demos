
await SlideDeck
    .Create("Slides")
    .TitleSlide("Slides", "A tiny fluent slide deck generator")
    .Slide("One file", "The generated deck is a complete HTML document with inline CSS.")
    .Slide("Fluid templates", "Fluid renders the HTML while the builder keeps the authoring API small.")
    .WriteToAsync("deck.html");
