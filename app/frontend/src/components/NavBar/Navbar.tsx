import React from 'react';
import "../../styles/Navbar.css"

function Navbar() {
    /*static navbar*/
    return (
        <div className="navbar">
            <div className="logo">
                <p className="logo-text"><strong>Power</strong> Company</p>
            </div>
            <div className="menu">
                <ul>
                    <li className="menu-list">Menu</li>
                    <li className="menu-list">Pay Bills</li>
                    <li className="menu-list">Outages</li>
                    <li className="menu-list">Support</li>
                    <li className="menu-list">Search</li>
                </ul>
            </div>
            <div className="misc">
                <ul>
                    <li className="menu-list">English </li>
                    <li className="menu-list">Account</li>
                </ul>
            </div>
            
        </div>

    );
}

export default Navbar;