package org.spicery.nutmeg.tokeniser;

import org.spicery.nutmeg.powerups.alert.Alert;
import org.spicery.nutmeg.powerups.charrepeater.CharRepeaterInterface;
import org.spicery.nutmeg.powerups.charrepeater.RecordingCharRepeater;
import org.spicery.nutmeg.powerups.charrepeater.RecordingCharRepeaterInterface;

public class Tokeniser<T> {

	TokenFactoryInterface<T> factory;
	RecordingCharRepeaterInterface cucharin;
	
	public Tokeniser( TokenFactoryInterface< T > factory, RecordingCharRepeaterInterface cucharin ) {
		super();
		this.factory = factory;
		this.cucharin = cucharin;
	}
	
	public Tokeniser( TokenFactoryInterface< T > factory, CharRepeaterInterface cucharin ) {
		super();
		this.factory = factory;
		this.cucharin = new RecordingCharRepeater( cucharin );
	}	
	
	//-----------------------------------------------------------------
	//	DELEGATED FUNCTIONS
	//-----------------------------------------------------------------



	char nextChar() {
		return this.cucharin.nextChar();
	}
	
	char nextChar( char ch ) {
		return this.cucharin.nextChar( ch );
	}
	
	void skipChar() {
		this.cucharin.skipChar();
	}
	
	void pushChar( char ch ) {
		this.cucharin.pushChar( ch );
	}
	
	char peekChar2( char otherwise ) {
		final char ch1 = this.nextChar( otherwise );
		final char ch2 = this.peekChar( otherwise );
		this.pushChar( ch1 );
		return ch2;
	}
	
	char peekChar() {
		return this.cucharin.peekChar();
	}
	
	char peekChar( char ch ) {
		return this.cucharin.peekChar( ch );
	}
	
	boolean isNextChar( char ch_want ) {
		return this.cucharin.isNextChar( ch_want );
	}
	
	boolean isNextString( String wanted ) {
		return this.cucharin.isNextString( wanted );
	}
	
	boolean hasNextChar() {
		return this.cucharin.hasNextChar();
	}
	
	boolean tryReadChar( final char ch_want ) {
		final boolean read = this.isNextChar( ch_want );		
		if ( read ) {
			this.skipChar();
		}
		return read;
	}
	
	// -----------------------------------------------------------------
	
	private static final char SINGLE_QUOTE = '\'';
	private static final char DOUBLE_QUOTE = '"';
	private static final char FORWARD_SLASH = '/';
	private static final char BACK_SLASH = '\\';
	final static int MAX_CHARACTER_ENTITY_LENGTH = 32;

	
	public T readToken() {
		this.eatWhiteSpace();
		if ( !this.hasNextChar() ) {
			return this.factory.endOfFile();
		} else {	
			final char pch = this.peekChar( '\0' );
			if ( Character.isLetter( pch ) ) {
				return this.gatherNonEmptyName();
			} else if ( ( pch == DOUBLE_QUOTE || pch == SINGLE_QUOTE ) ) {
				return this.gatherString();
			} else if ( Character.isDigit( pch ) || pch == '-' ) {
				return this.gatherNumber();
			} else {
				throw Alert.unimplemented();
			}
		}
	}
	
	T gatherNumber() {
		cucharin.startRecording();
		this.nextChar();
		for (;;) {
			char ch = this.nextChar( '\0' );
			if ( !Character.isDigit( ch ) ) break;
		}
		cucharin.backUp();
		String original = cucharin.stopRecording();
		return this.factory.intToken( original, Integer.parseInt( original ) );
	}
	
	boolean isNameChar( final char ch ) {
		return Character.isLetterOrDigit( ch ) || ch == '-' || ch == '.';
	}
	
	T gatherNonEmptyName() {
		final StringBuilder name = new StringBuilder();
		while ( this.hasNextChar() ) {
			final char ch = this.nextChar();
			if ( isNameChar( ch ) ) {
				name.append( ch );
			} else {
				this.pushChar( ch );
				break;
			}
		}
		if ( name.length() == 0 ) {
			throw new Alert( "Name missing" );
		}
		
		return factory.nameToken( name.toString() );
	}
	
	T gatherString() {
		cucharin.startRecording();
		final StringBuilder attr = new StringBuilder();
		final char opening_quote_mark = this.nextChar();
		if ( opening_quote_mark != DOUBLE_QUOTE && opening_quote_mark != SINGLE_QUOTE ) throw new Alert( "Attribute value not quoted" ).culprit( "Character", opening_quote_mark );
		for (;;) {
			char ch = this.nextChar();
			if ( ch == opening_quote_mark ) break;
			if ( ch == '\\' ) {
				attr.append( this.readJSONStyleEscapeChar() );
			} else {
				attr.append( ch );
			}
		}
		String original = cucharin.stopRecording();
		return factory.stringToken( original, attr.toString() );
	}
	
	char readJSONStyleEscapeChar() {
		final char ch = this.nextChar();
		switch ( ch ) {
			case SINGLE_QUOTE:
			case DOUBLE_QUOTE:
			case FORWARD_SLASH:
			case BACK_SLASH:
				return ch;
			case 'n':
				return '\n';
			case 'r':
				return '\r';
			case 't':
				return '\t';
			case 'f':
				return '\f';
			case 'b':
				return '\b';
			case '&':
				return this.readXMLStyleEscapeChar();
			default:
				return ch;
		}
	}
	
	char readXMLStyleEscapeChar() {
		if ( this.tryReadChar( BACK_SLASH ) ) {
			return this.readJSONStyleEscapeChar();
		} else {
			final String esc = this.readEscapeContent();
			if ( esc.length() >= 2 && esc.charAt( 0 ) == '#' ) {
				try {
					final int n = Integer.parseInt( esc.toString().substring( 1 ) );
					return (char)n;
				} catch ( NumberFormatException e ) {
					throw new Alert( "Unexpected numeric sequence after &#", e ).culprit( "Sequence", esc );
				}
			} else {
				return this.entityLookup( esc );
			}
		}
	}
	
	char entityLookup( final String symbol ) {
		final Character c = Lookup.lookup( symbol );
		if ( c != null ) {
			return c;
		} else {
			throw new Alert( "Unexpected escape sequence after &" ).culprit( "Sequence", symbol );
		}
	}
	
	String readEscapeContent() {
		final StringBuilder esc = new StringBuilder();
		for (;;) {
			final char ch = this.nextChar();
			if ( ch == ';' ) break;
			esc.append( ch );
			if ( esc.length() > MAX_CHARACTER_ENTITY_LENGTH ) {
				throw new Alert( "Malformed escape" ).culprit( "Sequence", esc );
			}
		}
		return esc.toString();
	}
	
	/**
	 * This skips over whitespace or comment-sequences.
	 */
	void eatWhiteSpace() {
		while ( this.hasNextChar() ) {
			final char ch = this.nextChar();
			if (ch == '#' && isNextString("##")) {
				this.skipChar();
				this.skipChar();
				while (true) {
					final char n = this.nextChar();
					if (n == '\n' || n == 'r') 
						break;
				}
		    } else if ( ! Character.isWhitespace( ch ) ) {
				this.pushChar( ch );
				return;
			}
		}
	}
	
}
