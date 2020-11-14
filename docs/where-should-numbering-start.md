Dijkstra [famously argued](https://www.cs.utexas.edu/users/EWD/transcriptions/EWD08xx/EWD831.html) that there is a preferred way of denoting an interval. He picked the subsequence 2, 3, ... 12 to illustrate his argument:
```
a)		2 ≤ i < 13       half-open at top
b)		1 < i ≤ 12       half-open at bottom
c)		2 ≤ i ≤ 12       closed
d)		1 < i < 13       open
```
His first observation was that conventions a) and b) have the advantage that the difference between the bounds as mentioned equals the length of the subsequence. This is true and quite an attractive property.

However, it overlooks the awkward fact that the end-point mentions a value that is not in the range. This feels especially wrong when dealing with implicit mappings. For example, if 0 = off, 1 = perhaps, 2 = definitely then we iterate over `0 ..< 3`, but 3 has no meaning. It's very uncomfortable to be introducing values that are guaranteed to be invalid.

Dijkstra also argues that, with a) and b), when one subsequence follows on from another, the upper bound of one equals the lower bound of the other. This is a really nice property and this is a persuasive point.

Again, this property hinges on the fact that the upper bound is not in the set of values. Hence if we want to represent the set of numbers { 1, 2, 3, 4, 5 } we will need the number 6 - where did that come from? It's not in the set. It's not the size of the set. It looks and feels very artifical. Or, to use Dijkstra's own 'boo-word', "ugly".

Is there a special case when this upper bound for a) and b) looks natural? Yes, there is one and only one such special case. When the lower bound is zero, the upper bound is the size of the set. We do not expect the size of the set to be a member of the set and the "ugliness" is resolved _for this special case_. 

From this, we can see that the preference for a half-open interval will lead us to want a lower bound of 0 (as opposed to 1). So Dijkstra will need to persuade us to embrace the counter-intuitive labelling of the first element of a sequence as the 0-th member.

And that's exactly where Dijkstra goes next - onto the defence of the choice of 0 as the label for the first member of a subsequence. He argues that there is "a smallest natural number" by which he means either 0 or 1, depending on your choice of 'natural'. He then argues that "Exclusion of the lower bound - as in b) and d) - forces for a subsequence starting at the smallest natural number the lower bound as mentioned into the realm of the unnatural numbers. That is ugly." In other words, if you choose the natural numbers starting at 0, you'll need a lower bound of -1 when you have 0 in the set and that's "ugly". Dijkstra eliminates b) and d) with this.

There's much to question about this, since for finite sequences an equally obvious domain is not the natural numbers but the set itself. And even if you do accept that the natural numbers are the right domain, there's always the awkward question of whether they start at 0 or 1. Let's call these Nat0 and Nat1 respectively. If we choose Nat0 then, yes, we end up with a lower bound of -1. But if you choose Nat1, then our lower bound is 0, which isn't "ugly" at all. 

Dijkstra then delivers something of a punch to force the choice between a) and c). He asks us to conside the upper bound for the empty sequence for a sequence starting at the smallest natural number: "inclusion of the upper bound would then force the latter to be unnatural by the time the sequence has shrunk to the empty one. That is ugly ...". This forces the choice of a).

Again, there's a sly reliance on taking Nat0 as our preferred basis for natural number. If we use c) then we're led to use Nat1 rather than Nat0 and the bounds are `1 <= i <= 0`. Again, 0 doesn't present itself as an obviously "ugly" number?

We follow Dijkstra in rejecting b) and d) as comfortable choices, simply because we strongly prefer the explicit inclusion of the initial member. But we see the choice between a) and c) as less about aesthetics and more about the primary use case, the positional labelling of the members of 1D arrays (aka lists, vectors, arrays). 

Most modern programming languages have done away with explicit lower bounds in favour of an implicit lower bound of 0 or 1. Depending on whether we make that implicit lower bound 0 or 1, we are pushed towards a) or c). We see the tradeoff as follows:

|    | Interval               | Pros                       | Cons                           |
| -- | ---------------------- | -------------------------- | ------------------------------ |
| a) | 0 <= i < length(list)  | Concatenation is easy      | First member is 0 not 1        |  
|    |                        | length = upper - lower     | Last item is A[length(A) - 1]  |
| c) | 1 <= i <= length(list) | Last item is A[length(A)]  | Concatenation is messy         |
|    |                        | First member is 1          | length = upper - lower + 1     |
|    |                        |                            | (only neat when lower = 1)     |

Which side of the tradeoff we prefer is clearly determined by the choice of implicit lower bound. In Nutmeg, because lists are indexed from 0, you'll find yourself using `0 ..< n` more often than `1 ... n`. So ultimately, Nutmeg sides with Dijkstra because 1) that's what most coders coming to Nutmeg will be familiar with and 2) we want to make it easy to hop backwards and forwards between Nutmeg and other programming languages.

But, when you use Nutmeg arrays, which support explicit lower bounds, you may choose to have a lower bound of 1. This usually happens when the labelling of elements used in the problem domain starts from 1. And when that happens, you'll find it more natural to use the closed range  `1 ... n`.
