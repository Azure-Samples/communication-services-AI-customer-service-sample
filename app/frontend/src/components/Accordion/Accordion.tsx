import React, {useEffect, useState} from 'react';
import AccordionItem from './AccordionItem';
import {getSummaryDetails, ConversationalInsights} from "../../utils/SummaryList"
import "../../styles/Accordion.css";


interface ThreadIdProps {
    threadId: string | null;
}
const Accordion: React.FC<ThreadIdProps> = ({ threadId }) => { 

   const [data, setData] = useState<ConversationalInsights>();

    useEffect(() => {
        getSummaryDetails(threadId).then(apiData => {
            setData(apiData);
        })
    }, [threadId])

    return (
        <div>
            <div className="accordion-header">
               &nbsp;&nbsp; Assistant
               <br/>
            </div>
        <div className="accordion">
            {data?.summaryItems.map((item, index) => (
                <AccordionItem key={index} {...item} />
            ))}
            </div>
        </div>
    );
};

export default Accordion;
