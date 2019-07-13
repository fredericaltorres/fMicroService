import React, { Component } from 'react';
import { LineChart, Line, CartesianGrid, XAxis, YAxis, Tooltip } from 'recharts';
import ReactTable from "react-table";
import "react-table/react-table.css";

// https://github.com/ayrton/react-key-handler
// https://www.npmjs.com/package/react-key-handler
import KeyHandler, { KEYPRESS, KEYDOWN, KEYUP } from 'react-key-handler'; 

/*
recharts
     http://recharts.org/en-US/examples/BiaxialLineChart
React-Table    
     https://www.npmjs.com/package/react-table
     https://codesandbox.io/s/react-table-simple-table-hpduw
     https://codesandbox.io/s/github/tannerlinsley/react-table/tree/master/archives/v6-examples/react-table-cell-renderers
 */

const chartWidth = 501;
const chartHeight = 175;

const dollarFormatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2
});

const numberFormatter = new Intl.NumberFormat('en-US', {
    maximumSignificantDigits: 3,
    minimumFractionDigits: 2
});

 export class Home extends Component {

    static displayName          = Home.name;
    timerId                     = null;
    refreshTimeOut              = 1000 * 5;    
    summaryTableDefaultPageSize = 6;

    state = {
        systemActivitySummary: {
            donationCountryBreakdown                       : {},
            donationSentToEndPointActivitySummaryDictionary: {}, 
            donationEnqueuedActivitySummaryDictionary      : {},
            dashboardResourceActivitySummaryDictionary     : {},            
            donationProcessedActivitySummaryDictionary     : {},
            donationInfoSummaryDictionary                  : {},
            donationErrorsSummaryDictionary                : {},                        
        },
        autoRefreshOn: false,
        donationCountryBreakdownMinimunAmountForDisplay: 1000,
        keyAltDown: false,
    };

    clearAllErrors = () => {

        return fetch('api/SystemActivities/GetSystemActivityClearError').then(response => response.json())
            .then(data => {
                console.log(`clearAllErrors data:${JSON.stringify(data)}`);
                this.updateState('systemActivitySummary', data);
            });
    }

    clearAll = () => {

        return fetch('api/SystemActivities/GetSystemActivityClearAll').then(response => response.json())
            .then(data => {
                console.log(`clearAll data:${JSON.stringify(data)}`);
                this.updateState('systemActivitySummary', data);
            });
    }

    reloadData = () => {

        return fetch('api/SystemActivities/GetSystemActivitySummary').then(response => response.json())
            .then(data => {
                console.log(`reloadData data:${JSON.stringify(data)}`);
                this.updateState('systemActivitySummary', data);
            });
    }

    componentDidMount() {
        
        this.reloadData().then(() => {

            this.setAutoRefresh(true);
        });        
    }

    updateState = (property, value) => {

        this.setState({ ...this.state, [property]: value });
    }

    reverseAutoRefresh = () => {

        this.setAutoRefresh(!this.state.autoRefreshOn);
    }

    getAutoRefreshStatus = () => {

        return this.state.autoRefreshOn ? "On" : "Off";
    }

    setAutoRefresh = (on) => {

        if (on) {
            this.timerId = setInterval(this.reloadData, this.refreshTimeOut);
        }
        else {
            clearTimeout(this.timerId);
        }
        this.updateState('autoRefreshOn', on);
    }    

    getActivitySummaryTable = (activitySummaryDictionary) => {

        let sTotal         = 0;
        let sItemPerSecond = 0;
        let sCaption       = null;
        const machineNames = Object.keys(activitySummaryDictionary);

        const r = machineNames.map((machineName) => {

            const machineInfo  = activitySummaryDictionary[machineName];
            sTotal            += machineInfo.total;
            sItemPerSecond    += machineInfo.itemPerSecond;

            if (sCaption === null) sCaption = machineInfo.caption;

            return {
                machineName  : machineInfo.machineName,
                caption      : machineInfo.caption,
                total        : machineInfo.total,
                itemPerSecond: machineInfo.itemPerSecond,
                jsonData     : machineInfo.jsonData,
                messages     : machineInfo.messages, // This is an array of string
            };
        });

        r.push({ // Add summation row
            machineName  : 'Total',
            caption      : sCaption,
            total        : sTotal,
            itemPerSecond: sItemPerSecond,
        });
        return r;
    }

    getDonationInfoActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationInfoSummaryDictionary);
    }

    getDonationErrorsActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationErrorsSummaryDictionary);
    }

    getDonationProcessedpointActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationProcessedActivitySummaryDictionary);
    }

    getDonationSentToEndpointActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationSentToEndPointActivitySummaryDictionary);
    }

    getDonationEnqueuedActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationEnqueuedActivitySummaryDictionary);
    }
    
    renderDonationInfoActivitySummaryTable = () => {

        const data = this.getDonationInfoActivitySummaryTable();
        if (data.length === 0)
            return null;

        return (
            <ReactTable
                data={data}
                columns={[{
                    Header: "Donation Info",
                    columns: this.getColumnsForMessagesTable()
                }]}
                defaultPageSize={this.summaryTableDefaultPageSize+2}
                className="-striped -highlight"
                showPagination={false}
                SubComponent={row => {
                    
                    var messages = row.original.messages;
                    var messagesHtml = messages.map((message, index) => {
                        return <li key={index}>{message}</li>;
                    });                    
                    return (
                        <ul style={{ padding: "20px" }}>
                            {messagesHtml}
                        </ul>
                    );
                }}
            />
        );
    }

    renderDonationErrorsActivitySummaryTable = () => {

        const data = this.getDonationErrorsActivitySummaryTable();
        if (data.length === 0)
            return null;

        return (
            <ReactTable
                data={data}
                columns={[{
                        Header: "Donation Errors",
                        columns: this.getColumnsForMessagesTable()
                }]}                
                defaultPageSize={this.summaryTableDefaultPageSize}
                className="-striped -highlight"
                showPagination={false}
                SubComponent={row => {
                    var messages = row.original.message;
                    var messagesHtml = messages.map((message, index) => {
                        return <li key={index}>{message}</li>;
                    });
                    return (
                        <ul style={{ padding: "20px" }}>
                            {messagesHtml}
                        </ul>
                    );
                }}
            />
        );
    }

    getColumnsForMessagesTable = () => [
        {
            Header: "Machine Name",
            id: "machineName",
            accessor: d => d.machineName,
        },
        { Header: "Total", accessor: "total" },
        { Header: "Messages", id: "message", accessor: d => d.message ? d.message[d.message.length - 1] : '' } // Return the last message that arrived
    ]

    getColumnsForDonationPerSecondTable = () => [
        {
            Header: "Machine Name",
            id: "machineName",
            accessor: d => d.machineName,
        },
        { Header: "Total", accessor: "total" },
        { Header: "Donation/S", accessor: "itemPerSecond" }
    ];

    renderDonationProcessedActivitySummaryTable = () => {

        const data = this.getDonationProcessedpointActivitySummaryTable();
        if (data.length === 0)
            return null;

        return (
            <ReactTable
                data={data}
                columns={[{
                        Header: "Donation Processed",
                        columns: this.getColumnsForDonationPerSecondTable()
                }]}                
                defaultPageSize={this.summaryTableDefaultPageSize}
                className="-striped -highlight"
                showPagination={false}
            />
        );
    }

    renderDonationSentToEndpointActivitySummaryTable = () => {

        const data = this.getDonationSentToEndpointActivitySummaryTable();
        if (data.length === 0)
            return null;

        return (
            <ReactTable
                data={data}
                columns={[{
                        Header: "Donation Sent To Endpoint",
                        columns: this.getColumnsForDonationPerSecondTable()
                }]}                
                defaultPageSize={this.summaryTableDefaultPageSize}
                className="-striped -highlight"
                showPagination={false}
            />
        );
    }

    renderDonationEnqueuedActivitySummaryTable = () => {

        const data = this.getDonationEnqueuedActivitySummaryTable();
        if (data.length === 0)
            return null;

        return (
            <ReactTable
                data={data}
                columns={[{
                    Header: "Donation Enqueued",
                    columns: this.getColumnsForDonationPerSecondTable()
                }]}
                defaultPageSize={this.summaryTableDefaultPageSize}
                className="-striped -highlight"
                showPagination={false}
            />
        );
    }
    
    getColumnsForJsonDataTable = () => [
        {
            Header: "Machine Name",
            id: "machineName",
            accessor: d => d.machineName.replace("person", ""),
        },
        { Header: "Total", accessor: "total" },
        { Header: "Json Data", accessor: "jsonData" }
    ];
    
    getDonationMachineCount = (dictionary) => {

        const machineNames = Object.keys(dictionary);
        return machineNames.length;
    };

    getDonationMachineName = (dictionary, machineIndex) => {

        const machineNames = Object.keys(dictionary);
        if (machineNames.length) {
            const machineName = machineNames[machineIndex];
            return machineName;
        }

        return null;
    };

    getCountryTotalAmountDonated = () => {

         const donationCountryBreakdown = this.state.systemActivitySummary.donationCountryBreakdown;
         const countries = Object.keys(donationCountryBreakdown);
         let total = 0;
         countries.forEach((country) => {

             const amount = Math.round(donationCountryBreakdown[country]);
             total += amount;
         });

         return total;
    }

    getCountryBreakDownChartData = () => {

        const donationCountryBreakdown = this.state.systemActivitySummary.donationCountryBreakdown;
        const countries = Object.keys(donationCountryBreakdown);
        const data = [];
        countries.forEach((country) => {

            const amount = Math.round(donationCountryBreakdown[country]);
            if (amount > this.state.donationCountryBreakdownMinimunAmountForDisplay)
                data.push({ country, amount });
        });
        data.sort((a, b) => (a.amount < b.amount) ? 1 : -1);

        return data;
    }

    getDonationChartData = (dictionary, machineIndex) => {
        
        const machineNames = Object.keys(dictionary);
        if (machineNames.length) {

            const machineName = machineNames[machineIndex];
            if (machineName) {

                const history = dictionary[machineName].history; // List<DonationActivityItem>
                const data = history.map((e) => {

                    return { timeStamp: e.chartLabel, value: e.total };
                });

                return data;
            }
            else {
                // Machine #2 may not be ready
            }
        }

        return [];
    };

    getDonationSentToEndPointChartData = (machineIndex) => {

        return this.getDonationChartData(this.state.systemActivitySummary.donationSentToEndPointActivitySummaryDictionary, machineIndex);
    };

    getDonationEnqueuedChartData = (machineIndex) => {

        return this.getDonationChartData(this.state.systemActivitySummary.donationEnqueuedActivitySummaryDictionary, machineIndex);
    };

    getDonationProcessedChartData = (machineIndex) => {

        return this.getDonationChartData(this.state.systemActivitySummary.donationProcessedActivitySummaryDictionary, machineIndex);
    };

    getDonationProcessedMachineName = (machineIndex) => {

        return <LineChart width={chartWidth} height={chartHeight} data={this.getDonationProcessedChartData(machineIndex)} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
            <Line type="monotone" dataKey="value" stroke="#8884d8" />
            <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
            <XAxis dataKey="timeStamp" />
            <YAxis />
            <Tooltip />
        </LineChart>;
    };

    getDonationEnqueuedMachineName = (machineIndex) => {

        return <LineChart width={chartWidth} height={chartHeight} data={this.getDonationEnqueuedChartData(machineIndex)} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
            <Line type="monotone" dataKey="value" stroke="#8884d8" />
            <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
            <XAxis dataKey="timeStamp" />
            <YAxis />
            <Tooltip />
        </LineChart>;
    };

    getDonationSentToEndPointMachineName = (machineIndex) => {

        return <LineChart width={chartWidth} height={chartHeight} data={this.getDonationSentToEndPointChartData(machineIndex)} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
            <Line type="monotone" dataKey="value" stroke="#8884d8" />
            <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
            <XAxis dataKey="timeStamp" />
            <YAxis />
            <Tooltip />
        </LineChart>;
    };

    getDonationCountryBreakdownChart = () => {
        const chartWidth = 1000;
        const chartHeight = 175;

        return <LineChart width={chartWidth} height={chartHeight} data={this.getCountryBreakDownChartData()} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
            <Line type="monotone" dataKey="amount" stroke="#8884d8" />
            <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
            <XAxis dataKey="country" />
            <YAxis />
            <Tooltip />
        </LineChart>;
    };
    
     getDonationPerformanceInfoChartsTR = (dictionary, title, generateChartCallBack) => {

        let max = this.getDonationMachineCount(dictionary);
        let html = [];
        for (let i = 0; i < max; i++) {
            html.push(<td key={`${title}-${i}`}>
                <div className="card">
                    <div className="card-header">{title}, machine {i} {this.getDonationMachineName(dictionary, i)}</div>
                    <div className="card-body">
                        {generateChartCallBack(i)}                        
                    </div>
                </div>
            </td>);
         }

        return html;
    }

     getDonationPerformanceInfoCharts = () => {

        return <table>
            <tbody>
                <tr>
                    {this.getDonationPerformanceInfoChartsTR(this.state.systemActivitySummary.donationSentToEndPointActivitySummaryDictionary, 'Donation Sent', this.getDonationSentToEndPointMachineName)}
                </tr>
                <tr><td colSpan="6">&nbsp;</td></tr>
                <tr>
                    {this.getDonationPerformanceInfoChartsTR(this.state.systemActivitySummary.donationEnqueuedActivitySummaryDictionary, 'Donation Enqueued', this.getDonationEnqueuedMachineName)}
                </tr>
                <tr><td colSpan="6">&nbsp;</td></tr>
                <tr>
                    {this.getDonationPerformanceInfoChartsTR(this.state.systemActivitySummary.donationProcessedActivitySummaryDictionary, 'Donation Processed ', this.getDonationProcessedMachineName)}
                </tr>
                <tr><td colSpan="6">&nbsp;</td></tr>
            </tbody>
        </table >
    }

    onDonationCountryBreakdownMinimunAmountForDisplayChange = (e) => {

        this.updateState('donationCountryBreakdownMinimunAmountForDisplay', e.target.value);
     }

     onKeyboardAutoRefresh = (event) => {         

         event.preventDefault();
         if (this.state.keyAltDown)
             this.reverseAutoRefresh();
     }

     onKeyboardRefresh = (event) => {

         event.preventDefault();
         if (this.state.keyAltDown)
             this.reloadData();
     }

     onKeyboardAltKey = (event, down) => {

         if (this.state.keyAltDown !== down) {
             // console.log(`onKeyboardAltKey down:${down}`);
             event.preventDefault();
             this.updateState('keyAltDown', down);
         }
     }

     // https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/code/code_values
     
     getKeyHandlers = () => {

         return <span>

             <KeyHandler keyEventName={KEYUP} code="KeyA" onKeyHandle={this.onKeyboardAutoRefresh} />
             <KeyHandler keyEventName={KEYUP} code="KeyR" onKeyHandle={this.onKeyboardRefresh} />

             <KeyHandler keyEventName={KEYDOWN} code="AltLeft"  onKeyHandle={(event) => { this.onKeyboardAltKey(event, true); }}    />
             <KeyHandler keyEventName={KEYUP}   code="AltLeft"  onKeyHandle={(event) => { this.onKeyboardAltKey(event, false); }}   />
             <KeyHandler keyEventName={KEYDOWN} code="AltRight" onKeyHandle={(event) => { this.onKeyboardAltKey(event, true); }}    />
             <KeyHandler keyEventName={KEYUP}   code="AltRight" onKeyHandle={(event) => { this.onKeyboardAltKey(event, false); }}   />
         </span>;
     }

    getDonationEnqueuedMaxCount = () => {

        var t = this.getDonationEnqueuedActivitySummaryTable();
        var lastRecord = t[t.length - 1];
        return lastRecord.total;
    }

    getDonationProcessedMaxCount = () => {

        var t = this.getDonationProcessedpointActivitySummaryTable();
        var lastRecord = t[t.length - 1];
        return lastRecord.total;
    }
    
    render() {

        return (
            <React.Fragment>

                {this.getKeyHandlers()}

                <div>
                    <button type="button" className="btn btn-primary  btn-sm " onClick={this.reverseAutoRefresh} > AutoRefresh~~~: {this.getAutoRefreshStatus()} (Alt+A)</button>
                    &nbsp;
                    <button type="button" className="btn btn-primary  btn-sm " onClick={this.reloadData} > Refresh (Alt+R)</button>
                    &nbsp;
                    <button type="button" className="btn btn-primary  btn-sm " onClick={this.clearAllErrors} > Clear Errors </button>
                    &nbsp;
                    <button type="button" className="btn btn-primary  btn-sm " onClick={this.clearAll} > Clear All </button>
                    &nbsp;
                    <small> {new Date().toString()} </small>

                    <br /><br />

                    <div className="card">
                        <div className="card-header">Summary</div>
                        <div className="card-body">
                            <center>
                                <h4>
                                    Donation Received: {numberFormatter.format(this.getDonationEnqueuedMaxCount())}
                                </h4>
                                <h4>
                                    Donation Processed: {numberFormatter.format(this.getDonationProcessedMaxCount())}
                                </h4>
                                <h4>
                                    Total Amount: {dollarFormatter.format(this.getCountryTotalAmountDonated())}
                                </h4>
                            </center>
                        </div>
                    </div>

                    <br />
             
                    <div className="card">
                        <div className="card-header">Countries Break Down 
                            &nbsp;&nbsp;&nbsp;                            
                            <small>                                
                                ( Minimun Amount:&nbsp;
                                <input id="donationCountryBreakdownMinimunAmountForDisplay" type="text" value={this.state.donationCountryBreakdownMinimunAmountForDisplay}
                                    onChange={this.onDonationCountryBreakdownMinimunAmountForDisplayChange}
                                /> )
                            </small>
                        </div>
                        <div className="card-body">
                            {this.getDonationCountryBreakdownChart()}
                        </div>
                    </div>

                    <br />

                    {this.getDonationPerformanceInfoCharts()}

                    {this.renderDonationSentToEndpointActivitySummaryTable()}
                    <br />
                    {this.renderDonationEnqueuedActivitySummaryTable()}                
                    <br />
                    {this.renderDonationProcessedActivitySummaryTable()}
                    <br />
                    {this.renderDonationInfoActivitySummaryTable()}
                    <br />
                    {this.renderDonationErrorsActivitySummaryTable()}
                    
                </div>
            </React.Fragment>
        );
    }
}

