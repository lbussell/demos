using Fluid;
using System.Text.Encodings.Web;

public sealed class SlideDeck
{
    private const string Template = """
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>{{ Title }}</title>
          <style>
            html, body { margin: 0; height: 100%; font-family: system-ui, sans-serif; color: black; background: white; }
            .deck { height: 100%; }
            .slide { box-sizing: border-box; min-height: 100%; padding: 10vmin; display: none; place-content: center; gap: 1.5rem; background: white; }
            .slide.active { display: grid; }
            .slide h1 { margin: 0; font-size: clamp(3rem, 10vw, 7rem); line-height: 1; }
            .slide p { margin: 0; max-width: 32ch; font-size: clamp(1.5rem, 4vw, 3rem); }
            .title { text-align: center; }
            .progress { position: fixed; left: 0; right: 0; bottom: 0; height: 1rem; background: white; }
            .progress-bar { height: 100%; width: 0; background: #2563eb; }
          </style>
        </head>
        <body>
          <main class="deck">
            {% for slide in Slides %}
            <section class="slide {{ slide.Kind }}">
              <h1>{{ slide.Title }}</h1>
              <p>{{ slide.Text }}</p>
            </section>
            {% endfor %}
          </main>
          <div class="progress" aria-hidden="true"><div class="progress-bar"></div></div>
          <script>
            const slides = [...document.querySelectorAll(".slide")];
            const progress = document.querySelector(".progress-bar");
            let current = 0;

            function show(index) {
              current = Math.max(0, Math.min(index, slides.length - 1));
              slides.forEach((slide, i) => slide.classList.toggle("active", i === current));
              progress.style.width = `${((current + 1) / slides.length) * 100}%`;
            }

            document.addEventListener("keydown", event => {
              if (event.key === "ArrowLeft" || (event.shiftKey && (event.key === " " || event.key === "Enter"))) {
                show(current - 1);
              } else if (event.key === "ArrowRight" || event.key === " " || event.key === "Enter") {
                show(current + 1);
              }
            });

            show(0);
          </script>
        </body>
        </html>
        """;

    private readonly string title;
    private readonly List<Slide> slides = [];

    private SlideDeck(string title) => this.title = title;

    public static SlideDeck Create(string title) => new(title);

    public SlideDeck TitleSlide(string title, string subtitle = "")
    {
        slides.Add(new Slide("title", title, subtitle));
        return this;
    }

    public SlideDeck Slide(string title, string body)
    {
        slides.Add(new Slide("regular", title, body));
        return this;
    }

    public async Task WriteToAsync(string path)
    {
        await File.WriteAllTextAsync(path, Render());
    }

    public string Render()
    {
        var parser = new FluidParser();
        var template = parser.Parse(Template);
        var options = new TemplateOptions();
        options.MemberAccessStrategy.Register<Slide>();

        var context = new TemplateContext(options, StringComparer.Ordinal)
            .SetValue("Title", title)
            .SetValue("Slides", slides);

        return template.Render(context, HtmlEncoder.Default);
    }
}

public sealed record Slide(string Kind, string Title, string Text);
