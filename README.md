Make sure to install the two fonts in this folder, otherwise you will only see boxes instead of text.


# Main features:
### Touch-compatibilty
    No tiny buttons and context menus
### Vertical Japanese text
    Also displays ruby/furigana correctly
### Fast
    No waiting times when changing reader size
### Ability to mark text
    Three colors possible
### Tense-sensitive dictionary
    Uses grammar tables to find possible base forms of conjugated words, and searches for all of them.
    Lookup is very fast, although the dictionary needs a few seconds to load (<5) at startup.

# Additional features:
    Library function
    Ability to skip to chapters
    Ability to easily jump between pages
    Ability to load pictures with a non-local(internet) source,
    which can happen when converting web pages to epub.
    Can replace all Hiragana with Katakana through option in
    the save file, useful if you're bad at Katakana

# Downsides:
### Only vertical Japanese text
    This Epub-reader only displays text in a vertical, right-to-left layout and
    the dictionary only works with Japanese. Using this reader with books not written
    in Japanese is not recommended, although Chinese may work.
### Special rendering
    This is not an epub-reader in the normal sense.
    Usually the html-files inside the epub are displayed using an html-renderer,
    which (I think so at least, I can't think of any other reason why so many reader-programms
    are so slow) makes everything quite slow, because the reader has to render
    the entire book at the same time in order to calculate the page count.
    This reader does something that looks very similar to a normal html-renderer,
    as long as you don't encounter html-elements that are uncommon in books, e.g tables.
    This custom rendering is very fast.
### Font needs to be installed
    You need to install a font in order to use this reader: I do not know how to include
    the font in the application for I am not a software developer.
    The font is included in the files (MSMINCHO.TTF).
