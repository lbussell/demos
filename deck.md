---
title: Slides
subtitle: A tiny Pandoc slide deck
aspectratio: 169
---

# Property Testing
@lbussell / loganbussell

# Background
- LLMs generate code really quickly
- The bottleneck is how quickly changes can be validated

# Problem
How do we get *absolute* confidence in what the LLM generated?

# What are property tests?

# Building generators

Start broad, then add domain constraints:

1. `Gen.String`
2. valid serialized text
3. domain-shaped values
4. composed domain objects
5. spec-backed property checks

# Example: Containers - SHA-256 digests

Generate the domain value first:

```csharp
Gen.Byte.Array[32, 32]
    .Select(bytes =>
        $"sha256:{Convert.ToHexString(bytes).ToLowerInvariant()}")
```

# Example: Containers

Build small generators and compose them:

- registry host
- repository component
- repository name
- tag reference
- SHA-256 digest
- full image specifier

# Guardrails
