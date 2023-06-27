/**
 * Copyright Stephen Leach, 2014
 * This file is part of the MinXML for Java library.
 * 
 * MinXML for Java is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Foobar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with MinXML for Java.  If not, see <http://www.gnu.org/licenses/>.
 *  
 */
package org.spicery.nutmeg.powerups.alert;

import java.io.PrintWriter;
import java.lang.reflect.Constructor;
import java.lang.reflect.InvocationTargetException;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.spicery.nutmeg.powerups.io.StringPrintWriter;

/**
 * Alerts are intended to act as generic application exceptions that can
 * be decorated with contextual key-value information. The idiom for using them
 * is: throw new Alert( message ).culprit( key1, value1 ).culprit( key2, value2 ); 
 *
 * As the exception propogates outward, it is appropriate for additional
 * information to be tagged onto the exception and for it to be re-thrown.
 * 
 */
public class Alert extends RuntimeException implements Iterable< Culprit > {

	private static final long serialVersionUID = -7054658959511420366L;
	private LinkedList< Culprit > culprit_list = new LinkedList< Culprit >();

	public Alert() {
		super();
	}

	/**
	 * This is the most general constructor, which simply invokes the super 
	 * class constructor with the same arguments.
	 * @param message the main message
	 * @param cause the originating throwable
	 */
	public Alert( final String message, final Throwable cause ) {
		super( message, cause );
	}

	public Alert( final String message ) {
		super( message );
	}

	public Alert( final Throwable cause ) {
		super( cause );
	}
	
	private void add( final Culprit culprit ) {
		this.culprit_list.add( culprit );
	}
	
	final static private Pattern varpattern = Pattern.compile( "(\\{[\\w]+\\})" );
	
	private String getValueFor( final String varname ) {
		final String varname1 = varname.substring( 1, varname.length() - 1 ); // clip off braces.
		for ( Culprit c : culprit_list ) {
			if ( varname1.equals( c.getKey() ) ) {
				return c.getValueString();
			}
		}
		return "";
	}
	
	private String substitute( String formatString ) {
		for (;;) {
			Matcher m = varpattern.matcher( formatString );
			if ( ! m.find() ) break;
			formatString = formatString.substring( 0, m.start(1) ) + getValueFor( m.group(1) ) + formatString.substring( m.end(1) ); 
		}
		return formatString;
	}
	
	public <T extends Exception> T wrap( final String formatString, Class< T > exnClass ) {
		try {
			Constructor< T > con = exnClass.getConstructor( String.class );
			return con.newInstance( this.substitute( formatString ) ); 
		} catch ( NoSuchMethodException | SecurityException | InstantiationException | IllegalAccessException | IllegalArgumentException | InvocationTargetException e ) {
			throw this.culprit( "Wrapping Alert failed", e );
		}
	}

	public <T extends Exception> T wrap( Class< T > exnClass ) {
		return this.wrap( this.getMessage(), exnClass );
	}

	/**
	 * This is a service method for reporting.
	 */
	public Iterator< Culprit > iterator() {
		return this.culprit_list.iterator(); 
	}
	
	public Culprit get( final String key ) {
		if ( key == null ) return null;
		for ( Culprit c : this.culprit_list ) {
			if ( key.equals(  c.getKey() ) ) {
				return c;
			}
		}
		return null;
	}
	
	public boolean hasKeyValue( String k, Object v ) {
		final Object x = this.get( k );
		return x == v || x != null && x.equals( v );
	}
	
	public boolean hasKey( String k ) {
		return this.get( k ) != null;
	}

	public Alert culprit( final String desc, final Object arg ) {
		this.add( new Culprit( desc, arg ) );
		return this;
	}

	public Alert culprit( final String desc, final int arg ) {
		return this.culprit( desc, new Integer( arg ) );
	}

	public Alert culprit( final String desc, final char ch ) {
		return this.culprit( desc, new Character( ch ) );
	}

	public Alert culprit( final Iterable< Culprit > list ) {
		for ( Culprit c : list ) {
			this.add( c );
		}
		return this;
	}

	public void reportTo( final PrintWriter pw ) {
		pw.print( "ALERT : " ); 
		pw.println( this.getMessage() );
		for ( Culprit c : this.culprit_list ) {
			c.output( pw );
		}
		pw.println( "" );
		pw.flush();
	}

	public void report() {
		this.reportTo( new PrintWriter( System.err ) );
	}
	
	public String toString() {
		final StringPrintWriter pw = new StringPrintWriter();
		this.reportTo( pw );
		return pw.toString();
	}

	//-- Special Keys -------------------------------------

	/**
	 * This is a convenient method for adding a culprit with
	 * the "hint" key. This should be used where there may be
	 * a common root cause that has an easy fix e.g. "Missing semi-colon?".
	 * @param msg The message hinting as to the fix.
	 * @return the original object.
	 */
	public Alert hint( final String msg ) {
		return this.culprit( "hint", msg );
	}
	
	public Alert code( final String string ) {
		return this.culprit( "code", string );
	}
	
	public Alert note( final String string ) {
		return this.culprit( "note", string );
	}
	
	public String getCodeString() {
		final Culprit c = this.get( "code" );
		return c == null ? null : (String)c.getValue();
	}

	//	---- This section just deals with the statics ----

	public static Alert unreachable() {
		return unreachable( "Internal error", (Throwable)null );
	}

	public static Alert unreachable( final Throwable t ) {
		return unreachable( "Internal error", t );
	}

	public static Alert unreachable( final String msg ) {
		return unreachable( msg, null );
	}

	/**
	 * A convenience method for building an Alert that is suitable
	 * for signalling code regions that the programmer intends should never arise.
	 * It's a useful subset of internal-error.
	 * @return the {@link Alert}
	 * 
	 * @param msg The main message
	 * @param t A wrapped throwable
	 */
	public static Alert unreachable( final String msg, final Throwable t ) {
		return new Alert( msg, t ).note( "A supposedly unreachable condition has been detected" );
	}

	/**
	 * A convenience method for building an Alert that is suitable
	 * to act as a placeholder for unfinished work. The message defaults
	 * to "Unimplemented".
	 * @return the {@link Alert}
	 */
	public static Alert unimplemented() {
		return unimplemented( "Unimplemented" );
	}

	/**
	 * A convenience method for building an Alert that is suitable
	 * to act as a placeholder for unfinished work.
	 * 
	 * @return the {@link Alert}
	 * @param msg the main message
	 */
	public static Alert unimplemented( final String msg ) {
		final Alert alert = new Alert( msg ).note( "An unimplemented feature has been reached" );
		alert.culprit( "message", msg );
		return alert;
	}

	/**
	 * A convenience method for building an Alert that is suitable
	 * for signalling that some part of the object state has entered
	 * a condition that the programmer intends to be impossible. The
	 * message is "Internal error".
	 * 
	 * @return the {@link Alert}
	 */
	public static Alert internalError() {
		return new Alert( "Internal Error" );
	}

	/**
	 * A convenience method for building an Alert that is suitable
	 * for signalling that some part of the object state has entered
	 * a condition that the programmer intends to be impossible. 
	 * 
	 * @return the {@link Alert}
	 * @param msg the main message
	 */
	public static Alert internalError( final String msg ) {
		return new Alert( "Internal Error" ).note( "This is an internal error" );
	}


}