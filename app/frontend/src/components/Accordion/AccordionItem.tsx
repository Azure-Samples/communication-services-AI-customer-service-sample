import React, { useState } from 'react';
import "../../styles/Accordion.css";
import { SummaryItem } from '../../utils/SummaryList';
export interface AccordionItemProps {
    key: string;
    value: string;
}

const AccordionItem: React.FC<SummaryItem> = ({ title, description }) => {
    const [isActive, setIsActive] = useState(false);

    const toggleAccordion = () => {
        setIsActive(!isActive);
    };

    return (
        <div className={`accordion-item ${isActive ? 'active' : ''}`}>
            <div onClick={toggleAccordion}>
                <span className={`chevron ${isActive ? 'down' : 'right'}`}>&#x3e;</span>
                <span className="accordion-name">{title}</span>
            </div>
            {isActive && <div className="accordion-content">{description}</div>}
        </div>
    );
};

export default AccordionItem;
